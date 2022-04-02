using Breeze.Sharp.Core;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Breeze.Sharp {

  /// <summary>
  /// Metadata information about a complex type.
  /// </summary>
  [DebuggerDisplay("{Name}")]
  public sealed class ComplexType: StructuralType, IJsonSerializable {

    public ComplexType(MetadataStore metadataStore) :base(metadataStore) {

    }

    internal override void UpdateFromJNode(JNode jNode, bool isFromServer) {
      Name = MetadataStore.GetStructuralTypeNameFromJNode(jNode, isFromServer);
      // BaseTypeName = jnode.Get<string>("baseTypeName");
      // IsAbstract = jnode.Get<bool>("isAbstract");
      jNode.GetJNodeArray("dataProperties").ForEach(jn => {
        var dpName = GetPropertyNameFromJNode(jn);
        var dp = GetDataProperty(dpName);
        dp.UpdateFromJNode(jn, isFromServer);
      });
      // validators
      // custom
    }
    
    public override bool IsEntityType {
      get { return false; }
    }

    JNode IJsonSerializable.ToJNode(object config) {
      var jo = new JNode();
      jo.AddPrimitive("shortName", ShortName);
      jo.AddPrimitive("namespace", Namespace);
      jo.AddPrimitive("isComplexType", true);
      // jo.AddProperty("baseTypeName", BaseTypeName);
      // jo.AddProperty("isAbstract", IsAbstract, false);
      jo.AddArray("dataProperties", DataProperties.Where(dp => dp.IsInherited == false));
      // jo.AddArrayProperty("validators", Validators);
      // jo.AddProperty("custom", Custom.ToJObject)
      return jo;
    }

  
   
  }

  /// <summary>
  /// For internal use only.
  /// </summary>
  internal class ComplexTypeCollection : KeyedCollection<string, ComplexType> {
    protected override string GetKeyForItem(ComplexType item) {
      return item.ShortName + ":#" + item.Namespace;
    }
  }

}
