﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.8.3928.0.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class configuration {
    
    private string modelField;
    
    private string areaField;
    
    private bool overlayField;
    
    private int minConfidenceField;
    
    public configuration() {
        this.modelField = "Inception";
        this.areaField = "";
        this.overlayField = true;
        this.minConfidenceField = 50;
    }
    
    /// <remarks/>
    public string Model {
        get {
            return this.modelField;
        }
        set {
            this.modelField = value;
        }
    }
    
    /// <remarks/>
    public string Area {
        get {
            return this.areaField;
        }
        set {
            this.areaField = value;
        }
    }
    
    /// <remarks/>
    public bool Overlay {
        get {
            return this.overlayField;
        }
        set {
            this.overlayField = value;
        }
    }
    
    /// <remarks/>
    public int MinConfidence {
        get {
            return this.minConfidenceField;
        }
        set {
            this.minConfidenceField = value;
        }
    }
}
