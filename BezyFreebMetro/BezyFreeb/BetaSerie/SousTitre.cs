﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :2.0.50727.8000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

//
// Ce code source a été automatiquement généré par xsd, Version=2.0.50727.3038.
//

namespace BezyFB
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class episode
    {
        private string show_idField;

        private string episode_idField;

        private string seasonField;

        private episode[] episode1Field;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string show_id
        {
            get { return this.show_idField; }
            set { this.show_idField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string episode_id
        {
            get { return this.episode_idField; }
            set { this.episode_idField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string season
        {
            get { return this.seasonField; }
            set { this.seasonField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("episode")]
        public episode[] episode1
        {
            get { return this.episode1Field; }
            set { this.episode1Field = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SousTitreRoot
    {
        private string errorsField;

        private rootSubtitlesSubtitle[] subtitlesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string errors
        {
            get { return this.errorsField; }
            set { this.errorsField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("subtitle", typeof(rootSubtitlesSubtitle), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public rootSubtitlesSubtitle[] subtitles
        {
            get { return this.subtitlesField; }
            set { this.subtitlesField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class rootSubtitlesSubtitle
    {
        private string idField;

        private string languageField;

        private string sourceField;

        private string qualityField;

        private string fileField;

        private string urlField;

        private string dateField;

        private episode[] episodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string id
        {
            get { return this.idField; }
            set { this.idField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string language
        {
            get { return this.languageField; }
            set { this.languageField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string source
        {
            get { return this.sourceField; }
            set { this.sourceField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string quality
        {
            get { return this.qualityField; }
            set { this.qualityField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string file
        {
            get { return this.fileField; }
            set { this.fileField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string url
        {
            get { return this.urlField; }
            set { this.urlField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string date
        {
            get { return this.dateField; }
            set { this.dateField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("episode")]
        public episode[] episode
        {
            get { return this.episodeField; }
            set { this.episodeField = value; }
        }
    }
}