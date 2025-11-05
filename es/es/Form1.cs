using System;
using System.Collections.Generic;
using System.Drawing;           
using System.Windows.Forms;     

namespace es                    
{
    public partial class Form1 : Form
    {

        Panel[] dischi;                    // Array che contiene i dischi

        int[] posDischi;                   //posizione di ogni disco 0 = sinistra, 1 = centro, 2 = destra

        int[] torreX;                      // Coordinate X del centro di ogni torre (sinistra, centro, destra)

        int baseY;                         // y dove partono e si devono fermare i dischi

        int nDischi;                       // dischi che si vogliono usare

        (int disco, int from, int to)[] mosse;     //tupla delle mosse

        int indiceMosse;                   // mossa in esecuzione

        Timer timer;                       // oggetto timer per animazione

        int fase = 0;                      // movimento 0 = sale, 1 = orizzontale, 2 = scende

        int targetX, targetY;              // coordinate che il disco deve raggiungere

        int discoCorrente = -1;            // disco che si sta muovendo

        public Form1()
        {
            InitializeComponent();             

            
            dischi = new Panel[]                   // creo un array con i dischi
            {
                pnl_dim_1, pnl_dim_2, pnl_dim_3, pnl_dim_4,  
                pnl_dim_5, pnl_dim_6, pnl_dim_7              
            };

            posDischi = new int[dischi.Length];    // Creo array per ricordare posizione di ogni disco


            torreX = new int[]                     // Salvo il centro X di ogni torre
            {
                pnl_torre_sinistra.Left + pnl_torre_sinistra.Width / 2,   // centro torre sinistra
                pnl_torre_centro.Left   + pnl_torre_centro.Width   / 2,   // centro torre centrale
                pnl_torre_destra.Left   + pnl_torre_destra.Width   / 2    // centro torre destra
            };


            baseY = (pnl_base != null) ? pnl_base.Top : this.ClientRectangle.Bottom - 40; //assegno il valore y della base


            timer = new Timer();                   // Creo un timer
            timer.Interval = 100;                   // ogni quanto
            timer.Tick += Timer_Tick;              // Quando scatta, chiama la funzione Timer_Tick
        }

        
        private void btn_avvia_Click(object sender, EventArgs e)
        {
            nDischi = (int)nmr_ndischi.Value;      // Legge quanti dischi vuole l'utente (dal NumericUpDown)

            for (int i = 0; i < dischi.Length; i++)
            {
                dischi[i].Visible = i < nDischi;    // Mostra solo i primi n dischi
            }

            
            for (int i = 0; i < nDischi; i++)
            {
                posDischi[i] = 0;                  // 0 = torre sinistra
            }

            
            for (int i = nDischi - 1; i >= 0; i--)
            {
                ImpilaDisco(i, 0);                 // Impila il disco i sulla torre 0 (sinistra)
            }

            
            mosse = new (int, int, int)[2048];     // Array grande a sufficienza (max 2047 mosse per 11 dischi)
            indiceMosse = 0;                       // Partiamo dalla mossa 0
            GeneraMosse(nDischi, 0, 2, 1);         // Muovi n dischi da torre 0 a torre 2 usando 1 come ausiliaria

            
            fase = 0;                              // Inizia dalla fase di salita
            discoCorrente = -1;                    // Nessun disco in movimento
            timer.Start();                         // Avvia il timer → animazione parte
        }

        
        private void ImpilaDisco(int index, int torre)
        {
            Panel d = dischi[index];               // Prendo il pannello del disco
            int altezza = d.Height;                // Altezza del disco (tutti uguali)

            // Conta quanti dischi sono già sulla torre (escludendo questo)
            int dischiSotto = 0;
            for (int i = 0; i < nDischi; i++)
            {
                if (i != index && posDischi[i] == torre)  // Se è sulla stessa torre e non è questo disco
                    dischiSotto++;
            }

            // Calcola Y: base - (dischi sotto + 1) * altezza
            int y = baseY - (dischiSotto + 1) * altezza;
            int x = torreX[torre] - d.Width / 2;   // Centra il disco sulla torre

            d.Left = x;                            // Imposta posizione X
            d.Top = y;                             // Imposta posizione Y
        }

        
        private void GeneraMosse(int n, int from, int to, int aux)
        {
            if (n == 0) return;                    // Caso base: 0 dischi → niente da fare

            // Passo 1: sposta n-1 dischi da "from" a "aux" (usando "to" come ausiliario)
            GeneraMosse(n - 1, from, aux, to);

            // Passo 2: sposta il disco più grande (n) da "from" a "to"
            mosse[indiceMosse++] = (n - 1, from, to);  // n-1 perché indice parte da 0

            // Passo 3: sposta n-1 dischi da "aux" a "to" (usando "from" come ausiliario)
            GeneraMosse(n - 1, aux, to, from);
        }

        
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (indiceMosse == 0)                  // Se non ci sono più mosse
            {
                timer.Stop();                      // Ferma l'animazione
                return;
            }

            // Se non stiamo muovendo nessun disco, prendi la prossima mossa
            if (fase == 0 && discoCorrente == -1)
            {
                discoCorrente = mosse[0].disco;    // Quale disco muovere
            }

            const int step = 8;                    // Quanti pixel si muove per frame (velocità)

            
            if (fase == 0)
            {
                // Muovi il disco verso l'alto
                dischi[discoCorrente].Top = Math.Max(dischi[discoCorrente].Top - step, 60);

                // Quando arriva in cima (Y=60), passa alla fase orizzontale
                if (dischi[discoCorrente].Top <= 60)
                {
                    fase = 1;
                    targetX = torreX[mosse[0].to] - dischi[discoCorrente].Width / 2;
                }
            }

            
            else if (fase == 1)
            {
                int left = dischi[discoCorrente].Left;

                // Muovi verso destra o sinistra
                if (left < targetX)
                    left = Math.Min(left + step, targetX);
                else if (left > targetX)
                    left = Math.Max(left - step, targetX);

                dischi[discoCorrente].Left = left;

                // Quando arriva al centro della torre di destinazione
                if (left == targetX)
                {
                    fase = 2;
                    int sotto = ContaDischiTorre(mosse[0].to);  // Quanti dischi già lì?
                    targetY = baseY - (sotto + 1) * dischi[discoCorrente].Height;
                }
            }

            
            else if (fase == 2)
            {
                dischi[discoCorrente].Top = Math.Min(dischi[discoCorrente].Top + step, targetY);

                // Quando tocca la posizione finale
                if (dischi[discoCorrente].Top >= targetY)
                {
                    dischi[discoCorrente].Top = targetY;           // Forza posizione esatta
                    posDischi[discoCorrente] = mosse[0].to;        // Aggiorna posizione logica

                    // Rimuovi la mossa completata dall'array
                    Array.Copy(mosse, 1, mosse, 0, --indiceMosse);

                    // Prepara la prossima mossa
                    fase = 0;
                    discoCorrente = -1;
                }
            }
        }

        
        private int ContaDischiTorre(int torre)
        {
            int count = 0;
            for (int i = 0; i < nDischi; i++)
            {
                // Se il disco è sulla torre E non è quello che stiamo muovendo
                if (i != discoCorrente && posDischi[i] == torre)
                    count++;
            }
            return count;
        }
    }
}