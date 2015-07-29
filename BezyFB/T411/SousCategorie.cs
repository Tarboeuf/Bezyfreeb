namespace BezyFB.T411
{
    public class SousCategorie
    {

        public SousCategorie(Category cat, string parentName)
        {
            Cat = cat;
            NameWithParent = parentName + " / " + cat.Name;
        }

        public Category Cat { get; set; }

        public string NameWithParent { get; set; }
    }
}