using System;
using AWS.Lambda.Powertools.Idempotency.Output;

namespace AWS.Lambda.Powertools.Idempotency;

/// <summary>
/// Create a builder that can be used to configure and create <see cref="IdempotencyOptions"/>
/// </summary>
public class IdempotencyOptionsBuilder
{
    private readonly int _localCacheMaxItems = 256;
    private bool _useLocalCache;
    private long _expirationInSeconds = 60 * 60; // 1 hour
    private string _eventKeyJmesPath;
    private string _payloadValidationJmesPath;
    private bool _throwOnNoIdempotencyKey;
    private string _hashFunction = "MD5";
    private ILog _log = new NullLog();

    /// <summary>
    /// Initialize and return an instance of IdempotencyConfig.
    /// Example:
    /// IdempotencyConfig.Builder().WithUseLocalCache().Build();
    /// This instance must then be passed to the Idempotency.Config:
    /// Idempotency.Config().WithConfig(config).Configure();
    /// </summary>
    /// <returns>an instance of IdempotencyConfig</returns>
    public IdempotencyOptions Build() =>
        new(_eventKeyJmesPath,
            _payloadValidationJmesPath,
            _throwOnNoIdempotencyKey,
            _useLocalCache,
            _localCacheMaxItems,
            _expirationInSeconds,
            _hashFunction,
            _log);

    /// <summary>
    /// A JMESPath expression to extract the idempotency key from the event record.
    /// See <a href="https://jmespath.org/">https://jmespath.org/</a> for more details.
    /// </summary>
    /// <param name="eventKeyJmesPath">path of the key in the Lambda event</param>
    /// <returns>the instance of the builder (to chain operations)</returns>
    public IdempotencyOptionsBuilder WithEventKeyJmesPath(string eventKeyJmesPath)
    {
        _eventKeyJmesPath = eventKeyJmesPath;
        return this;
    }

    /// <summary>
    /// Whether to locally cache idempotency results, by default false
    /// </summary>
    /// <param name="useLocalCache">Indicate if a local cache must be used in addition to the persistence store.</param>
    /// <returns>the instance of the builder (to chain operations)</returns>
    public IdempotencyOptionsBuilder WithUseLocalCache(bool useLocalCache)
    {
        _useLocalCache = useLocalCache;
        return this;
    }

    /// <summary>
    /// A JMESPath expression to extract the payload to be validated from the event record.
    /// See <a href="https://jmespath.org/">https://jmespath.org/</a> for more details.
    /// </summary>
    /// <param name="payloadValidationJmesPath">JMES Path of a part of the payload to be used for validation</param>
    /// <returns>the instance of the builder (to chain operations)</returns>
    public IdempotencyOptionsBuilder WithPayloadValidationJmesPath(string payloadValidationJmesPath)
    {
        _payloadValidationJmesPath = payloadValidationJmesPath;
        return this;
    }

    /// <summary>
    /// Whether to throw an exception if no idempotency key was found in the request, by default false
    /// </summary>
    /// <param name="throwOnNoIdempotencyKey">boolean to indicate if we must throw an Exception when
    ///                                idempotency key could not be found in the payload.</param>
    /// <returns>the instance of the builder (to chain operations)</returns>
    public IdempotencyOptionsBuilder WithThrowOnNoIdempotencyKey(bool throwOnNoIdempotencyKey)
    {
        _throwOnNoIdempotencyKey = throwOnNoIdempotencyKey;
        return this;
    }

    /// <summary>
    /// The number of seconds to wait before a record is expired
    /// </summary>
    /// <param name="duration">expiration of the record in the store</param>
    /// <returns>the instance of the builder (to chain operations)</returns>
    public IdempotencyOptionsBuilder WithExpiration(TimeSpan duration)
    {
        _expirationInSeconds = (long) duration.TotalSeconds;
        return this;
    }

    /// <summary>
    /// Function to use for calculating hashes, by default MD5.
    /// </summary>
    /// <param name="hashFunction">Can be any algorithm supported by HashAlgorithm.Create</param>
    /// <returns>the instance of the builder (to chain operations)</returns>
    public IdempotencyOptionsBuilder WithHashFunction(string hashFunction)
    {
        _hashFunction = hashFunction;
        return this;
    }

    /// <summary>
    /// Logs to a custom logger.
    /// </summary>
    /// <param name="log">The logger.</param>
    /// <returns>the instance of the builder (to chain operations)</returns>
    public IdempotencyOptionsBuilder LogTo(ILog log)
    {
        _log = log;
        return this;
    }
}