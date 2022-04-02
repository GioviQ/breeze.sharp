
namespace Breeze.Sharp {

  /// <summary>
  /// A QueryOptions instance is used to specify the 'options' under which a query will occur.
  /// </summary>
  public class QueryOptions : IJsonSerializable {

    public QueryOptions() {

    }
    
    public QueryOptions(JNode jNode) {
      FetchStrategy = jNode.GetNullableEnum<FetchStrategy>("fetchStrategy");
      MergeStrategy = jNode.GetNullableEnum<MergeStrategy>("mergeStrategy");
      
    }

    JNode IJsonSerializable.ToJNode(object config) {
      var jn = new JNode();
      jn.AddEnum("fetchStrategy", FetchStrategy );
      jn.AddEnum("mergeStrategy", MergeStrategy );
      return jn;
    }

    public QueryOptions(FetchStrategy? fetchStrategy, MergeStrategy? mergeStrategy) {
      FetchStrategy = fetchStrategy;
      MergeStrategy = mergeStrategy;
    }

    /// <summary>
    /// Returns a new QueryOptions based on this QueryOptions but with the specified FetchStrategy
    /// </summary>
    /// <param name="fetchStrategy"></param>
    /// <returns></returns>
    public QueryOptions With(FetchStrategy fetchStrategy) {
      return new QueryOptions(fetchStrategy, MergeStrategy);
    }

    /// <summary>
    /// Returns a new QueryOptions based on this QueryOptions but with the specified MergeStrategy
    /// </summary>
    /// <param name="mergeStrategy"></param>
    /// <returns></returns>
    public QueryOptions With(MergeStrategy mergeStrategy) {
      return new QueryOptions(FetchStrategy, mergeStrategy);
    }

    public static QueryOptions Default = new QueryOptions(Breeze.Sharp.FetchStrategy.FromServer, Breeze.Sharp.MergeStrategy.PreserveChanges);

    public FetchStrategy? FetchStrategy { get; internal set; }
    public MergeStrategy? MergeStrategy { get; internal set; }
    
  }

 
}
