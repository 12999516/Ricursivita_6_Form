using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace es
{
    public partial class Form1 : Form
    {
        List<Panel> pln;                    // lista dei pannelli
        List<(char dd, char ad)> mosse;     // lista delle mosse
        Timer timer; //timer

        int[] posDischi;      // posizione logica dei dischi: 0=A,1=B,2=C
        int[] torreX;//x dei pali dove impilare i dischi
        int baseY; //y dove partono e si fermano i pannelli
        int nDischi; //numero di pannelli
        int currentMove; //mossa che sta essendo eseguita

        public Form1()
        {
            InitializeComponent();
            pln = new List<Panel>(); //creo nuova lista di pannelli
            mosse = new List<(char dd, char ad)>(); //creo nuova lista di tuple
            timer = new Timer(); //creo nuovo timer
            timer.Interval = 1000; //ogni quanto viene chiamato tick
            timer.Tick += Timer_Tick;//mi metto in ascolto dell'evento chiamato tick
            mostra();//funzione per mostrare all'inizio nient e e popolare la lista di panel
        }

        private void mostra()
        {//popolo la lista di panel
            pln.Add(pnl_dim_1);
            pln.Add(pnl_dim_2);
            pln.Add(pnl_dim_3);
            pln.Add(pnl_dim_4);
            pln.Add(pnl_dim_5);
            pln.Add(pnl_dim_6);
            pln.Add(pnl_dim_7);
            pln.Add(pnl_dim_8);
            pln.Add(pnl_dim_9);

            //itero la lista e metto tutto invisibile
            for (int i = 0; i < pln.Count; i++)
            {
                pln[i].Visible = false;
            }
        }

        private void mostrar(int n)
        {
            // mostra solo i primi n pannelli
            for (int i = 0; i < n; i++)
            {
                pln[i].Visible = true;
            }
        }

        private void btn_avvia_Click(object sender, EventArgs e)
        {
            // numero dischi e visibilità
            nDischi = Math.Max(0, Math.Min(pln.Count, (int)nmr_ndischi.Value));
            mostrar(nDischi);//chiama la funzione per mostrare i primi n dischi

            posDischi = new int[nDischi];
            for (int i = 0; i < nDischi; i++) posDischi[i] = -1;

            torreX = new int[]//coordinate delle torri(sinistra centro destra)
            {
                pnl_torre_sinistra.Left + pnl_torre_sinistra.Width / 2,
                pnl_torre_centro.Left   + pnl_torre_centro.Width   / 2,
                pnl_torre_destra.Left   + pnl_torre_destra.Width   / 2
            };

            //assegno la base d'appoggio
            baseY = pnl_base.Location.Y;

            // posiziono graficamente i dischi sulla torre A
            for (int i = nDischi - 1; i >= 0; i--)//ciclo decrescente perché più grande sotto
            {
                ImpilaDisco(i, 0);//chiamo la funzione per impilare i dischi
            }

            mosse.Clear();//pulisco la lista di mosse nel caso
            hanoi(nDischi, 'A', 'C', 'B');//chiamo la funzione hanoi ossia quella ricorsiva

            //reparto animazione
            currentMove = 0; //inizializzo la mossa attuale a 
            if (mosse.Count > 0)//se le mosse sono più di 0
            {
                timer.Start();//iniza a contare il timer
            }
        }

        // funzione ricorsiva per le mosse e la logica del gioco
        private void hanoi(int n, char dd, char ad, char app)
        {
            if (n == 1)//se c'è un solo disco allora sto lavorando sul disco più in alto
            {
                mosse.Add((dd, ad));//aggiungo la mossa da dove va a dove va
            }
            else
            {
                hanoi(n - 1, dd, app, ad);//se non è così hanoi chiama se stessa salendo di un "livello" rimanendo nella
                                          //stessa colonna mettendolo nella colonna ex di appoggio che ora diventa obbiettivo
                                          //e usando l'obbiettivo attuale come appoggio
                mosse.Add((dd, ad));//crea la mossa come prima
                hanoi(n - 1, app, ad, dd);//poi chiama hanoi sasalendo di un "livello" cambiando
                                          //colonna e passando a quella che ora è l'appoggio
                                          //il disco viene messo nella ex colonna di destinazione usando quella attuale di
                                          //partenza come appoggio
            }
        }

        // funzione per creare la "torre" di partenza
        private void ImpilaDisco(int index, int torre)//funzione per impilare i dischi con index che rappresenta il disco
                                                      //e torre che rappresenta dove metterlo
        {
            if (index < 0 || index >= pln.Count) return;//se l'indice è miore di zero o maggiore uguale di pln.count ritorna
            //se non è così
            Panel d = pln[index];//d prende il panel all'indice index
            if (d == null) return;//se d non esiste ritorna
            int altezza = d.Height;//l'altezza è l'altezza di d

            int dischiSotto = 0;//variabile per gli eventuali dischi sottostanti
            for (int i = 0; i < nDischi; i++)//itera per il numero di dischi
            {
                if (i != index && posDischi[i] == torre) dischiSotto++;//se i è diverso da index e posdischi[i] è uguale a torre si aggiunge un disco sotto
            }

            int y = baseY - (dischiSotto + 1) * altezza;//calcola la posizione verticale del disco in base al numero di dischi
                                                        //già presenti nella torre
            int x = torreX[torre] - d.Width / 2;//calcola la posizione orizzontale centrando il disco rispetto alla torre

            d.Left = x;//la distanza dal bordo sinistro viene impostata a x
            d.Top = y;// la distanza dal bordo superiore viene impostata a y
            posDischi[index] = torre;//la posizione del disco a posizione index è impostata a torre
        }

        //evento per il tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentMove >= mosse.Count)//se la mossa attuale è maggiore uguale del mosse.count
            {
                timer.Stop();//viene fermato il timer
                return;//stoppa la funzione
            }

            var mv = mosse[currentMove];//mv prende la mossa attuale
            int from = CharToTower(mv.dd);//il dd prende il chartotower a cui viene passato la partenza della mossa
            int to = CharToTower(mv.ad);//il ad prende il chartotower a cui viene passato la destinazione della mossa

            int found = -1;//indice del disco da spostare
            int bestTop = int.MaxValue;//la coordinata top più vicina al top
            for (int i = 0; i < nDischi; i++)//itera i dischi
            {
                if (posDischi[i] == from)//se posdischi in posizione i è uguale a dd
                {
                    int t = pln[i].Top;//t prende la distanza dal sopra del panel in posizione i
                    if (t < bestTop)//se t è maggiore della massima distanza dal top
                    {
                        bestTop = t;//il top prende il nuovo valore
                        found = i;//found prende il i
                    }
                }
            }

            // nessun disco da spostare: salta
            if (found == -1)
            {
                currentMove++;//viene incrementata la mossa attuale
                return;
            }

            Panel p = pln[found];//p prende il panel in posizione found

            // calcola posizione di destinazione (stacking) prima di aggiornare posDischi
            int sotto = ContaDischiTorre(to); // conta dischi già presenti su 'to'
            int targetY = baseY - (sotto + 1) * p.Height;
            int targetX = torreX[to] - p.Width / 2;

            // teletrasporto: posizione istantanea
            p.Left = targetX;
            p.Top = targetY;

            // aggiorna stato logico
            posDischi[found] = to;//la posdischi in posizione found prende to

            // passo successivo
            currentMove++;
        }

        private int ContaDischiTorre(int torre)
        {
            int count = 0;//inizializza il contatore a 0
            for (int i = 0; i < nDischi; i++)//itera i panel 
                if (posDischi[i] == torre) count++;//se il posdischi in posizione i è uguale a torre il contatore prende più uno
            return count;//ritorna il contatore
        }


        //funzione per associare al char di hanoi il valore
        private int CharToTower(char c)
        {
            string cc = c.ToString().ToLower();
            if (cc == "a")
            {
                return 0;
            }
            else if (cc == "b")
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }
}