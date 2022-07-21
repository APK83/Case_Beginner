using System;
using System.Collections.Generic;



namespace NetChecker
{
    class Urls
    {
        public string Adress { get; set; }
        public Urls()
        {
            Adress = "Adress";
        }
        public Urls(string _adress)
        {
            Adress = _adress;
        }
        public virtual void Show()
        {
            Console.WriteLine($"{Adress}");
        }
        public List<Urls> _adress = new List<Urls>();
        public override string ToString() => Adress;

    }
}
