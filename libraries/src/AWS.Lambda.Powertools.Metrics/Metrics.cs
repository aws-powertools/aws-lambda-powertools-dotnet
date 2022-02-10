﻿/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using AWS.Lambda.Powertools.Common;

namespace AWS.Lambda.Powertools.Metrics;

/// <summary>
///     Class Metrics.
///     Implements the <see cref="IMetrics" />
/// </summary>
/// <seealso cref="IMetrics" />
public class Metrics : IMetrics
{
    /// <summary>
    ///     The instance
    /// </summary>
    private static IMetrics _instance;

    /// <summary>
    ///     The context
    /// </summary>
    private readonly MetricsContext _context;

    /// <summary>
    ///     The power tools configurations
    /// </summary>
    private readonly IPowertoolsConfigurations _powertoolsConfigurations;

    /// <summary>
    ///     If true, Powertools will throw an exception on empty metrics when trying to flush
    /// </summary>
    private readonly bool _raiseOnEmptyMetrics;

    /// <summary>
    ///     Creates a Metrics object that provides features to send metrics to Amazon Cloudwatch using the Embedded metric
    ///     format (EMF). See
    ///     https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/CloudWatch_Embedded_Metric_Format_Specification.html
    /// </summary>
    /// <param name="powertoolsConfigurations">Lambda Powertools Configuration</param>
    /// <param name="nameSpace">Metrics Namespace Identifier</param>
    /// <param name="service">Metrics Service Name</param>
    /// <param name="raiseOnEmptyMetrics">Instructs metrics validation to throw exception if no metrics are provided</param>
    internal Metrics(IPowertoolsConfigurations powertoolsConfigurations, string nameSpace = null, string service = null,
        bool raiseOnEmptyMetrics = false)
    {
        if (_instance != null) return;

        _instance = this;
        _powertoolsConfigurations = powertoolsConfigurations;
        _raiseOnEmptyMetrics = raiseOnEmptyMetrics;
        _context = InitializeContext(nameSpace, service, null);
    }

    /// <summary>
    ///     Implements interface that adds new metric to memory.
    /// </summary>
    /// <param name="key">Metric Key</param>
    /// <param name="value">Metric Value</param>
    /// <param name="unit">Metric Unit</param>
    /// <exception cref="System.ArgumentNullException">
    ///     'AddMetric' method requires a valid metrics key. 'Null' or empty values
    ///     are not allowed.
    /// </exception>
    void IMetrics.AddMetric(string key, double value, MetricUnit unit)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(
                "'AddMetric' method requires a valid metrics key. 'Null' or empty values are not allowed.");
        
        if (value < 0) {
            throw new ArgumentException(
                "'AddMetric' method requires a valid metrics value. Value must be >= 0.");
        }

        if (_context.GetMetrics().Count == 100) _instance.Flush(true);

        _context.AddMetric(key, value, unit);
    }

    /// <summary>
    ///     Implements interface that sets metrics namespace identifier.
    /// </summary>
    /// <param name="nameSpace">Metrics Namespace Identifier</param>
    void IMetrics.SetNamespace(string nameSpace)
    {
        _context.SetNamespace(nameSpace);
    }

    /// <summary>
    ///     Implements interface that allows retrieval of namespace identifier.
    /// </summary>
    /// <returns>Namespace identifier</returns>
    string IMetrics.GetNamespace()
    {
        return _context.GetNamespace();
    }

    /// <summary>
    ///     Implements interface to get service name
    /// </summary>
    /// <returns>System.String.</returns>
    string IMetrics.GetService()
    {
        return _context.GetService();
    }

    /// <summary>
    ///     Implements interface that adds a dimension.
    /// </summary>
    /// <param name="key">Dimension key. Must not be null, empty or whitespace</param>
    /// <param name="value">Dimension value</param>
    /// <exception cref="System.ArgumentNullException">
    ///     'AddDimension' method requires a valid dimension key. 'Null' or empty
    ///     values are not allowed.
    /// </exception>
    void IMetrics.AddDimension(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(
                "'AddDimension' method requires a valid dimension key. 'Null' or empty values are not allowed.");

        _context.AddDimension(key, value);
    }

    /// <summary>
    ///     Implements interface that adds metadata.
    /// </summary>
    /// <param name="key">Metadata key. Must not be null, empty or whitespace</param>
    /// <param name="value">Metadata value</param>
    /// <exception cref="System.ArgumentNullException">
    ///     'AddMetadata' method requires a valid metadata key. 'Null' or empty
    ///     values are not allowed.
    /// </exception>
    void IMetrics.AddMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(
                "'AddMetadata' method requires a valid metadata key. 'Null' or empty values are not allowed.");

        _context.AddMetadata(key, value);
    }

    /// <summary>
    ///     Implements interface that sets default dimension list
    /// </summary>
    /// <param name="defaultDimensions">Default Dimension List</param>
    /// <exception cref="System.ArgumentNullException">
    ///     'SetDefaultDimensions' method requires a valid key pair. 'Null' or empty
    ///     values are not allowed.
    /// </exception>
    void IMetrics.SetDefaultDimensions(Dictionary<string, string> defaultDimensions)
    {
        foreach (var item in defaultDimensions)
            if (string.IsNullOrWhiteSpace(item.Key) || string.IsNullOrWhiteSpace(item.Value))
                throw new ArgumentNullException(
                    "'SetDefaultDimensions' method requires a valid key pair. 'Null' or empty values are not allowed.");

        _context.SetDefaultDimensions(DictionaryToList(defaultDimensions));
    }

    /// <summary>
    ///     Flushes metrics in Embedded Metric Format (EMF) to Standard Output. In Lambda, this output is collected
    ///     automatically and sent to Cloudwatch.
    /// </summary>
    /// <param name="metricsOverflow">If enabled, non-default dimensions are cleared after flushing metrics</param>
    /// <exception cref="global.SchemaValidationException">true</exception>
    void IMetrics.Flush(bool metricsOverflow)
    {
        if (_context.GetMetrics().Count == 0
            && _raiseOnEmptyMetrics)
            throw new SchemaValidationException(true);

        if (_context.IsSerializable)
        {
            var emfPayload = _context.Serialize();

            Console.WriteLine(emfPayload);

            _context.ClearMetrics();

            if (!metricsOverflow) _context.ClearNonDefaultDimensions();
        }
        else
        {
            Console.WriteLine(
                "##WARNING## Metrics and Metadata have not been specified. No data will be sent to Cloudwatch Metrics.");
        }
    }

    /// <summary>
    ///     Serialize global context object
    /// </summary>
    /// <returns>Serialized global context object</returns>
    public string Serialize()
    {
        return _context.Serialize();
    }

    /// <summary>
    ///     Implements the interface that pushes single metric to CloudWatch using Embedded Metric Format. This can be used to
    ///     push metrics with a different context.
    /// </summary>
    /// <param name="metricName">Metric Name. Metric key cannot be null, empty or whitespace</param>
    /// <param name="value">Metric Value</param>
    /// <param name="unit">Metric Unit</param>
    /// <param name="nameSpace">Metric Namespace</param>
    /// <param name="service">Service Name</param>
    /// <param name="defaultDimensions">Default dimensions list</param>
    /// <exception cref="System.ArgumentNullException">
    ///     'PushSingleMetric' method requires a valid metrics key. 'Null' or empty
    ///     values are not allowed.
    /// </exception>
    void IMetrics.PushSingleMetric(string metricName, double value, MetricUnit unit, string nameSpace, string service,
        Dictionary<string, string> defaultDimensions)
    {
        if (string.IsNullOrWhiteSpace(metricName))
            throw new ArgumentNullException(
                "'PushSingleMetric' method requires a valid metrics key. 'Null' or empty values are not allowed.");

        using var context = InitializeContext(nameSpace, service, defaultDimensions);
        context.AddMetric(metricName, value, unit);

        Flush(context);
    }

    /// <summary>
    ///     Implementation of IDisposable interface
    /// </summary>
    public void Dispose()
    {
        _instance.Flush();
    }

    /// <summary>
    ///     Adds new metric to memory.
    /// </summary>
    /// <param name="key">Metric Key. Must not be null, empty or whitespace</param>
    /// <param name="value">Metric Value</param>
    /// <param name="unit">Metric Unit</param>
    public static void AddMetric(string key, double value, MetricUnit unit = MetricUnit.None)
    {
        _instance.AddMetric(key, value, unit);
    }

    /// <summary>
    ///     Sets metrics namespace identifier.
    /// </summary>
    /// <param name="nameSpace">Metrics Namespace Identifier</param>
    public static void SetNamespace(string nameSpace)
    {
        _instance.SetNamespace(nameSpace);
    }

    /// <summary>
    ///     Retrieves namespace identifier.
    /// </summary>
    /// <returns>Namespace identifier</returns>
    public static string GetNamespace()
    {
        return _instance.GetNamespace();
    }

    /// <summary>
    ///     Adds new dimension to memory.
    /// </summary>
    /// <param name="key">Dimension key. Must not be null, empty or whitespace.</param>
    /// <param name="value">Dimension value</param>
    public static void AddDimension(string key, string value)
    {
        _instance.AddDimension(key, value);
    }

    /// <summary>
    ///     Adds metadata to memory.
    /// </summary>
    /// <param name="key">Metadata key. Must not be null, empty or whitespace</param>
    /// <param name="value">Metadata value</param>
    public static void AddMetadata(string key, object value)
    {
        _instance.AddMetadata(key, value);
    }

    /// <summary>
    ///     Set default dimension list
    /// </summary>
    /// <param name="defaultDimensions">Default Dimension List</param>
    public static void SetDefaultDimensions(Dictionary<string, string> defaultDimensions)
    {
        _instance.SetDefaultDimensions(defaultDimensions);
    }

    /// <summary>
    ///     Flushes metrics in Embedded Metric Format (EMF) to Standard Output. In Lambda, this output is collected
    ///     automatically and sent to Cloudwatch.
    /// </summary>
    /// <param name="context">If context is provided it is serialized instead of the global context object</param>
    private void Flush(MetricsContext context)
    {
        var emfPayload = context.Serialize();

        Console.WriteLine(emfPayload);
    }

    /// <summary>
    ///     Pushes single metric to CloudWatch using Embedded Metric Format. This can be used to push metrics with a different
    ///     context.
    /// </summary>
    /// <param name="metricName">Metric Name. Metric key cannot be null, empty or whitespace</param>
    /// <param name="value">Metric Value</param>
    /// <param name="unit">Metric Unit</param>
    /// <param name="nameSpace">Metric Namespace</param>
    /// <param name="service">Service Name</param>
    /// <param name="defaultDimensions">Default dimensions list</param>
    public static void PushSingleMetric(string metricName, double value, MetricUnit unit, string nameSpace = null,
        string service = null, Dictionary<string, string> defaultDimensions = null)
    {
        _instance.PushSingleMetric(metricName, value, unit, nameSpace, service, defaultDimensions);
    }

    /// <summary>
    ///     Sets global namespace, service name and default dimensions list. Service name is automatically added as a default
    ///     dimension
    /// </summary>
    /// <param name="nameSpace">Metrics namespace</param>
    /// <param name="service">Service Name</param>
    /// <param name="defaultDimensions">Default Dimensions List</param>
    /// <returns>MetricsContext.</returns>
    private MetricsContext InitializeContext(string nameSpace, string service,
        Dictionary<string, string> defaultDimensions)
    {
        var context = new MetricsContext();

        context.SetNamespace(!string.IsNullOrWhiteSpace(nameSpace)
            ? nameSpace
            : _powertoolsConfigurations.MetricsNamespace);

        context.SetService(!string.IsNullOrWhiteSpace(service)
            ? service
            : _powertoolsConfigurations.Service);

        var defaultDimensionsList = DictionaryToList(defaultDimensions);

        // Add service as a default dimension
        defaultDimensionsList.Add(new DimensionSet("Service", context.GetService()));

        context.SetDefaultDimensions(defaultDimensionsList);

        return context;
    }

    /// <summary>
    ///     Helper method to convert default dimensions dictionary to list
    /// </summary>
    /// <param name="defaultDimensions">Default dimensions dictionary</param>
    /// <returns>Default dimensions list</returns>
    private List<DimensionSet> DictionaryToList(Dictionary<string, string> defaultDimensions)
    {
        var defaultDimensionsList = new List<DimensionSet>();
        if (defaultDimensions != null)
            foreach (var item in defaultDimensions)
                defaultDimensionsList.Add(new DimensionSet(item.Key, item.Value));

        return defaultDimensionsList;
    }

    /// <summary>
    ///     Helper method for testing purposes. Clears static instance between test execution
    /// </summary>
    internal static void ResetForTest()
    {
        _instance = null;
    }
}
