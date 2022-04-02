namespace Breeze.Sharp {
  /// <summary>
  /// An enum used when retrieving entities to determine where to look
  /// </summary>
  public enum FetchStrategy {
    /// <summary>
    /// Retrieve entities from the cache only.
    /// </summary>
    FromLocalCache,
    /// <summary>
    /// Retrieve entities from the back-end data source only.
    /// </summary>
    FromServer,


  }
}
