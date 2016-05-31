using System;
using System.Threading.Tasks;
using CommonPortableLib;

namespace BezyFreebTest.Data
{
    public  class TestMessageDialog : IMessageDialogService
    {
        public async Task AfficherMessage(string v)
        {
            Console.WriteLine("__________________________________");
            Console.WriteLine(v);
            Console.WriteLine("__________________________________");
        }

        public virtual async Task<DialogResult> ShowYesNoDialog(string v)
        {
            return DialogResult.Yes;
        }
    }
}