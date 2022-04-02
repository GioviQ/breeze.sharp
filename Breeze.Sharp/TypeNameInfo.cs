using System;
using System.Linq;


namespace Breeze.Sharp {

  /// <summary>
  /// Class used to describe, parse and convert between EntityType/ComplexType names and CLR type names.
  /// </summary>
  public class TypeNameInfo {

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="shortName"></param>
    /// <param name="ns"></param>
    /// <param name="isAnonymous"></param>
    public TypeNameInfo(string shortName, string ns, bool isAnonymous = false) {
      ShortName = shortName;
      Namespace = ns;
      StructuralTypeName = ToStructuralTypeName(shortName, ns);
      IsAnonymous = isAnonymous;
    }

    /// <summary>
    /// /// Returns the TypeNameInfo for a specified CLR type.
    /// </summary>
    /// <param name="clrType"></param>
    /// <returns></returns>
    public static TypeNameInfo FromClrType(Type clrType) {
      return FromClrTypeName(clrType.FullName);
    }

    /// <summary>
    /// Returns the TypeNameInfo for a specified CLR type name.
    /// </summary>
    /// <param name="clrTypeName"></param>
    /// <returns></returns>
    public static TypeNameInfo FromClrTypeName(string clrTypeName) {
      if (clrTypeName.StartsWith(MetadataStore.ANONTYPE_PREFIX)) {
        return new TypeNameInfo(clrTypeName, string.Empty, true);
      }

      var entityTypeNameNoAssembly = clrTypeName.Split(',')[0];
      var nameParts = entityTypeNameNoAssembly.Split('.');
      string ns;
      var shortName = nameParts[nameParts.Length - 1];
      if (nameParts.Length > 1) {
        ns = string.Join(".", nameParts.Take(nameParts.Length - 1));
      } else {
        ns = "";
      }
      return new TypeNameInfo(shortName, ns);
    }

    /// <summary>
    /// Returns the TypeNameInfo for a specified structural type (EntityType or ComplexType) name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static TypeNameInfo FromStructuralTypeName(string name) {
      string shortName, ns;
      var ix = name.IndexOf(":#");
      if (ix == -1) {
        shortName = name;
        ns = string.Empty;
      } else {
        shortName = name.Substring(0, ix);
        ns = name.Substring(ix + 2);
      }
      return new TypeNameInfo(shortName, ns);
    }

    /// <summary>
    /// Returns the structual type name for a specified shortName and namespace. 
    /// </summary>
    /// <param name="shortName"></param>
    /// <param name="ns"></param>
    /// <returns></returns>
    public static string ToStructuralTypeName(string shortName, string ns) {
      if (string.IsNullOrEmpty(ns)) {
        return shortName;
      } else {
        return shortName + ":#" + ns;
      }
    }

    // This would be the same as the Name property;
    //public string ToStructualTypeName() {
    //  return ToStructuralTypeName(ShortName, Namespace);
    //}
    

    public TypeNameInfo ToClient(MetadataStore metadataStore) {
      return metadataStore.NamingConvention.ServerTypeNameToClient(this);
    }

    public TypeNameInfo ToServer(MetadataStore metadataStore) {
      return metadataStore.NamingConvention.ClientTypeNameToServer(this);
    }

    public static bool IsQualifiedTypeName(string typeName) {
      return typeName.IndexOf(":#", StringComparison.Ordinal) >= 0;
    }

    public string StructuralTypeName { get; private set; }
    public string ShortName { get; set; }
    public string Namespace { get; set; }
    public bool IsAnonymous { get; private set; }

  }


}
