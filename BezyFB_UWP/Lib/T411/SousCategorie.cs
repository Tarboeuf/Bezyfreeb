namespace BezyFB_UWP.Lib.T411
{
    public class SousCategorie
    {
        public SousCategorie(Category cat, string parentName)
        {
            Cat = cat;
            if (null != cat)
                NameWithParent = parentName + " / " + cat.Name;
            else
                NameWithParent = parentName;
        }

        public Category Cat { get; set; }

        public string NameWithParent { get; set; }
    }
}