using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2021aoc14
{
    class Program
    {
        class Naiv_Polimer
        {
            Dictionary<string, char> szabály;
            string elemei;
            public Naiv_Polimer(string path) 
            {
                szabály = new Dictionary<string, char>();
                string[] input = System.IO.File.ReadAllLines(path);
                elemei = input[0];
                foreach (string[] pár in input.Skip(2).Select(s => s.Replace(" -> ", "-").Split('-')))
                    szabály[pár[0]] = pár[1][0];
            }
            public void Polimerizál(int N)
            {
                Console.WriteLine($"Template: {elemei}");
                for (int n = 1; n <= N; n++)
                {
                    string beillesztések = "";
                    for (int i = 0; i < elemei.Length-1; i++)
                        beillesztések += szabály[elemei.Substring(i, 2)];
                    elemei = elemei[0].ToString()+String.Join("", elemei.Skip(1).Zip(beillesztések, (x, y) => y.ToString() + x.ToString()));

                    //Console.WriteLine($"After step {n}: {elemei}");
                    Statisztika();
                }
            }
            public void Statisztika()
            {
                Dictionary<char, int> kimutatás = new Dictionary<char, int>();
                foreach (char c in elemei)
                {
                    if (kimutatás.ContainsKey(c))
                        kimutatás[c]++;
                    else
                        kimutatás[c] = 1;
                }

                foreach (char c in kimutatás.Keys)
                    Console.WriteLine($"{c}: {kimutatás[c]} db");

                int max = kimutatás.Max(p=>p.Value);
                int min = kimutatás.Min(p => p.Value);
                Console.WriteLine($"{max} - {min} = {max-min}");
                Console.WriteLine("---------------------------------------------");
            }
        }

        class Okos_Polimer 
        {
            Dictionary<string, (string, string)> gyártás_gyár;
            Dictionary<string, char> gyártás_termék;
            Dictionary<string, ulong> nyilvántartás_gyár;
            Dictionary<char, ulong> nyilvántartás_termék;
            public Okos_Polimer(string path)
            {
                gyártás_gyár = new Dictionary<string, (string, string)>();
                gyártás_termék = new Dictionary<string, char>();
                nyilvántartás_gyár = new Dictionary<string, ulong>();
                nyilvántartás_termék = new Dictionary<char, ulong>();

                string[] input = System.IO.File.ReadAllLines(path);

                // szabályok értelmezése
                foreach (string[] pár in input.Skip(2).Select(s => s.Replace(" -> ", "-").Split('-')))
                {
                    gyártás_termék[pár[0]] = pár[1][0]; // NN -> C
                    gyártás_gyár[pár[0]] = ($"{pár[0][0]}{pár[1][0]}", $"{pár[1][0]}{pár[0][1]}"); // NN -> (NC, CN)
                }

                // kiindulási polimer "katalogizálása"
                string kiindulópolimer = input[0];

                // Group By: terméknyilvantartás a kezdőpolimer alapján
                // ilyet csinál: 
                // N: 2 
                // C: 1 
                // B: 1 

                foreach (char betű in gyártás_termék.Values)
                    nyilvántartás_termék[betű] = 0; 
                foreach (char betű in kiindulópolimer)
                    nyilvántartás_termék[betű]++;

                // Group By: gyárnyilvantartás a kezdőpolimer alapján
                // ilyet csinál: 
                // CH: 0 
                // HH: 0 
                // ...   
                // NN: 1 
                // NC: 1 
                // CB: 1 
                // ...   

                foreach (string gyár in gyártás_gyár.Keys)
                    nyilvántartás_gyár[gyár] = 0;
                for (int i = 0; i < kiindulópolimer.Length-1; i++)
                    nyilvántartás_gyár[kiindulópolimer.Substring(i, 2)]++;
            }

            public void Polimerizál(int N)
            {
                string[] nyilvántartás_gyár_kulcsok = nyilvántartás_gyár.Keys.ToArray(); // ez olyan hülyeség, hogy kell C#-ban...
                Diagnosztika(0);
                for (int n = 1; n <= N; n++)
                {
                    // A gyárak legyártják a termékeket: (terméknyilvántartás update)

                    foreach (string gyár in nyilvántartás_gyár.Keys)
                        nyilvántartás_termék[gyártás_termék[gyár]] += nyilvántartás_gyár[gyár];

                    // A gyárak legyártják a gyárakat: (új gyárnyilvántartás!)
                    Dictionary<string, ulong> új_nyilvántartás_gyár = new Dictionary<string, ulong>();
                    foreach (string gyár in gyártás_gyár.Keys)
                        új_nyilvántartás_gyár[gyár] = 0; 
                    foreach (string gyár in nyilvántartás_gyár_kulcsok)
                    {
                        (string balgyár, string jobbgyár) = gyártás_gyár[gyár];
                        új_nyilvántartás_gyár[balgyár] += nyilvántartás_gyár[gyár];
                        új_nyilvántartás_gyár[jobbgyár] += nyilvántartás_gyár[gyár];
                    }
                    nyilvántartás_gyár = új_nyilvántartás_gyár;
                    Diagnosztika(n);
                }
            }

            public void Diagnosztika(int n)
            {
                Console.WriteLine($"=========== {n}. lépés =========== ");
                Console.WriteLine($"------- gyárnyilvántartás ------- ");
                foreach (string gyár in nyilvántartás_gyár.Keys)
                    if (nyilvántartás_gyár[gyár] > 0)
                        Console.WriteLine($"{gyár}: {nyilvántartás_gyár[gyár]}");
                Console.WriteLine($"------- terméknyilvántartás ------- ");
                foreach (char termék in nyilvántartás_termék.Keys)
                    if (nyilvántartás_termék[termék] > 0)
                        Console.WriteLine($"{termék}: {nyilvántartás_termék[termék]}");
                ulong max = nyilvántartás_termék.Max(p => p.Value);
                ulong min = nyilvántartás_termék.Min(p => p.Value);
                //long sum = nyilvántartás_termék.Sum(p => p.Value); // long is túlcsordul :(
                Console.WriteLine($"------- terjedelem ------- ");
                Console.WriteLine($"{max} - {min} = {max - min}");
            }
        }

        static void Main(string[] args)
        {

            /** /
            Naiv_Polimer polimer = new Naiv_Polimer("teszt.txt");
            //
            Naiv_Polimer polimer = new Naiv_Polimer("input.txt");
            /**/

            /** /
            Okos_Polimer polimer = new Okos_Polimer("teszt.txt");
            /*/
            Okos_Polimer polimer = new Okos_Polimer("input.txt");
            /**/

            polimer.Polimerizál(40);
            Console.ReadKey();
        }
    }
}
