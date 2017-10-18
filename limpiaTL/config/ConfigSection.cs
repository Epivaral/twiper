using System.Configuration;

namespace limpiaTL
{
    public class OAuthConfigSection : ConfigurationSection
    {
        public const string SectionName = "OAuthConfigSection";

        private const string OAuthCollectionName = "OAuthConfigNames";

        [ConfigurationProperty(OAuthCollectionName)]
        [ConfigurationCollection(typeof(OAuthElementCollection), AddItemName = "add")]
        public OAuthElementCollection OAuthNames { get { return (OAuthElementCollection)base[OAuthCollectionName]; } }

    }

    public class OAuthElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new OAuthElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OAuthElement)element).name;
        }
    }

    public class OAuthElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("consumerKey", IsRequired = true)]
        public string consumerKey
        {
            get { return (string)this["consumerKey"]; }
            set { this["consumerKey"] = value; }
        }

        [ConfigurationProperty("consumerSecret", IsRequired = true)]
        public string consumerSecret
        {
            get { return (string)this["consumerSecret"]; }
            set { this["consumerSecret"] = value; }
        }

        [ConfigurationProperty("accessToken", IsRequired = true)]
        public string accessToken
        {
            get { return (string)this["accessToken"]; }
            set { this["accessToken"] = value; }
        }

        [ConfigurationProperty("accessTokenSecret", IsRequired = true)]
        public string accessTokenSecret
        {
            get { return (string)this["accessTokenSecret"]; }
            set { this["accessTokenSecret"] = value; }
        }

    }
}
