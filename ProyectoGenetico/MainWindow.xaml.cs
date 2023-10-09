using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProyectoGenetico
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<(int, int)> coordenadas = new List<(int, int)>();
        private bool primerPunto = true;
        private string matrizMostrar = "";
        private int[,] distancias = new int[1, 1];
        private int cantidadPuntos = 0;
        private int cantPoblación;
        private int[,] Población = new int[1, 1];
        private Random rand = new Random();
        private int[] mejorSolucionGlobal = new int[1];
        private int[,] Población2 = new int[1, 1];
        private string mejor = "";
        private int probCruzamiento;
        private int probMutación;
        private bool seHizoCruzamiento = false;
        private int nCiclos;
        private int ciclo;
        private bool esPob1Actual = true;
        private int búsquedasSinMejora;
        private double intentosSinMejora;

        private Dictionary<int, int> poblacionesChicas = new Dictionary<int, int> {
            { 1, 1 },
            { 2, 1 },
            { 3, 1 },
            { 4, 6 },
            { 5, 24 },
            { 6, 60 },
        };

        public MainWindow()
        {
            InitializeComponent();            
        }

        private void Canvas_PintarPunto(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(canvas);

            Ellipse ellipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(ellipse, mousePosition.X-4);
            Canvas.SetTop(ellipse, mousePosition.Y-3);

            if (primerPunto)
            {
                ellipse.Fill = Brushes.Red;
                primerPunto = false;
            }
            else
            {
                ellipse.Fill = Brushes.Black;
            }

            cantidadPuntos++; 
            canvas.Children.Add(ellipse);
            coordenadas.Add(((int)mousePosition.X, (int)mousePosition.Y));

            btnMostrar.IsEnabled = true;
        }

        private async void Ejecutar(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            canvas.IsEnabled = false;
            seHizoCruzamiento = false;
            ciclo = 0; 

            await Task.Run(ObtenerDistancias);

            //elegir cantidad de población para cuando son menos de 7 ciudades
            // y establecer 100 de población por default
            try
            {
                cantPoblación = Convert.ToInt32(nPoblacion.Text);
            }
            catch (Exception)
            {
                cantPoblación = 500;
            }
            if(cantidadPuntos < 7)
            {
                cantPoblación = poblacionesChicas[cantidadPuntos];
            }            

            try
            {
                probCruzamiento = Convert.ToInt32(ProbCruzamiento.Text);
            }
            catch (Exception)
            {
                probCruzamiento = 90;
            }

            try
            {
                probMutación = Convert.ToInt32(ProbMutación.Text);
            }
            catch (Exception)
            {
                probMutación = 20;
            }

            try
            {
                nCiclos = Convert.ToInt32(Ciclos.Text);
            }
            catch (Exception)
            {
                nCiclos = 100;
            }
            Población = new int[cantPoblación, cantidadPuntos + 2];
            Población2 = new int[cantPoblación, cantidadPuntos + 2];
            mejorSolucionGlobal = new int[cantidadPuntos + 2];
            mejorSolucionGlobal[cantidadPuntos + 1] = 999999999;
            intentosSinMejora = Math.Pow(cantidadPuntos, 2) * 0.33;

            await Task.Run(() => {
                InicializarPoblación();
                GenerarPobInicial();
                CalcularAptitud(Población);                
            });

            do
            {
                await Task.Run(() => {
                    //pobContraria es la población de salida o receptora o donde se guarda lo más nuevo
                    if (esPob1Actual)
                    {
                        ProcesoSelección(Población, Población2);
                        BuscarMejorSolución(Población2);
                        esPob1Actual = !esPob1Actual;
                    }
                    else
                    {
                        ProcesoSelección(Población2, Población);
                        BuscarMejorSolución(Población);
                        esPob1Actual = !esPob1Actual;
                    }

                    //pobContraria es la población de salida o donde se guarda lo más nuevo
                    if (esPob1Actual)
                    {
                        ProcesoCruzamiento(Población, Población2);
                        if (seHizoCruzamiento)
                        {
                            CalcularAptitud(Población2);
                            BuscarMejorSolución(Población2);
                            esPob1Actual = !esPob1Actual;
                        }
                    }
                    else
                    {
                        ProcesoCruzamiento(Población2, Población);
                        if (seHizoCruzamiento)
                        {
                            CalcularAptitud(Población);
                            BuscarMejorSolución(Población);
                            esPob1Actual = !esPob1Actual;
                        }
                    }                                       
                });

                //Cambios a los listBox
                //listBox.Items.Clear();
                //listBox2.Items.Clear();
                //listBoxDistancias.Items.Clear();

                if (ProcesoMutación())
                {
                    //await Task.Run(() =>
                    //{
                    //    MostrarRutasPob(distancias, listBoxDistancias, cantidadPuntos, cantidadPuntos - 2);
                    //});

                    if (esPob1Actual)
                    {
                        await Task.Run(() =>
                        {
                            //Mostrar antes de que se hagan los cambios y después
                            //MostrarRutasPob(Población, listBox, cantPoblación, cantidadPuntos);
                            MutaciónSwap(Población);
                            CalcularAptitud(Población);
                            BuscarMejorSolución(Población);
                            //MostrarRutasPob(Población, listBox2, cantPoblación, cantidadPuntos);
                        });
                        //TituloPob1.Text = "Población 1: (antes de mutación)";
                        //TituloPob2.Text = "Población 1: (después de mutación)";
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            //Mostrar antes de que se hagan los cambios y después
                            //MostrarRutasPob(Población2, listBox, cantPoblación, cantidadPuntos);
                            MutaciónSwap(Población2);
                            CalcularAptitud(Población2);
                            BuscarMejorSolución(Población2);
                            //MostrarRutasPob(Población2, listBox2, cantPoblación, cantidadPuntos);
                        });
                        //TituloPob1.Text = "Población 2: (antes de mutación)";
                        //TituloPob2.Text = "Población 2: (después de mutación)";
                    }
                }
                else
                {
                    if (seHizoCruzamiento)
                    {
                        //Mostrar el resultado del cruzamiento
                        //TituloPob1.Text = "Población 1: (después del cruzamiento)";
                        //TituloPob2.Text = "Población 2:";
                    }
                    else
                    {
                        //Mostrar el resultado de la selección solamente
                        //TituloPob1.Text = "Población 1:";
                        //TituloPob2.Text = "Población 2: (Después de la selección)";

                    }
                    await Task.Run(() =>
                    {
                        //MostrarRutasPob(Población, listBox, cantPoblación, cantidadPuntos);
                        //MostrarRutasPob(Población2, listBox2, cantPoblación, cantidadPuntos);
                        //MostrarRutasPob(distancias, listBoxDistancias, cantidadPuntos, cantidadPuntos - 2);
                    });
                }

                //Cambios al canvas
                if(ciclo % 5 == 0)
                {
                    canvas.Children.Clear();
                    Thread th1 = new Thread(() =>
                    {
                        RedibujarPuntos();
                        DibujarRuta();
                    });
                    th1.Start();
                    Thread th2 = new Thread(MostrarMejor);
                    th2.Start();
                }
                

                ciclo++;
                tBoxGen.Text = ciclo.ToString();
            } while (ciclo < nCiclos && búsquedasSinMejora < 666);            

            canvas.IsEnabled = true;
            Cursor = Cursors.Arrow;
        }

        private void ObtenerDistancias()
        {
            cantidadPuntos = coordenadas.Count;
            distancias = new int[cantidadPuntos, cantidadPuntos];
            int x1, x2, y1, y2;
            for (int a = 0; a < cantidadPuntos; a++)
                for (int b = 0; b < cantidadPuntos; b++)
                {
                    x1 = coordenadas[a].Item1;
                    y1 = coordenadas[a].Item2;
                    x2 = coordenadas[b].Item1;
                    y2 = coordenadas[b].Item2;
                    distancias[a, b] = Convert.ToInt32(Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2)));
                }
        }

        #region Población
        private void InicializarPoblación()
        {
            for(int a = 0; a < cantPoblación; a++)
            {
                for(int b = 0; b < cantidadPuntos; b++)
                {
                    Población[a, b] = b;
                }
                Población[a, cantidadPuntos] = 0;
            }
        }

        private void GenerarPobInicial()
        {
            for (int a = 0; a < cantPoblación; a++)
            {
                for (int b = 1; b < cantidadPuntos; b++)
                {
                    int temp = Población[a, b];
                    int random = rand.Next(1, cantidadPuntos - 1);
                    Población[a, b] = Población[a, random];
                    Población[a, random] = temp;
                    
                }
                Población[a, cantidadPuntos] = 0;
                Población[a, cantidadPuntos +1] = 0;
            }
        }

        private void CalcularAptitud(int[,] pob)
        {            
            for (int a = 0; a < cantPoblación; a++)
            {
                int aptitud = 0;
                for (int b = 1; b < cantidadPuntos; b++)
                {
                    aptitud += distancias[pob[a, b], pob[a, b +1]];
                }
                pob[a, cantidadPuntos +1] = aptitud;
            }
        }
        #endregion

        private void ProcesoSelección(int[,] pob, int[,] pobContraria)
        {                      
            for(int a = 0;a < cantPoblación; a++)
            {
                int r = rand.Next(0,cantPoblación - 1);
                if (pob[a, cantidadPuntos + 1] < pob[r, cantidadPuntos + 1])
                {
                    CopiarSolucion(a, a, pob, pobContraria);                    
                }
                else
                {
                    CopiarSolucion(r, a, pob, pobContraria);
                }
            }
        }

        #region Búsqueda de soluciones
        private void CopiarSolucion(int filaGanadora, int filaActual, int[,] pob, int[,] pobContraria)
        {
            for(int a = 0; a < cantidadPuntos+2; a++)
            {
                pobContraria[filaActual, a] = pob[filaGanadora, a];
            }
        }

        private void GuardarMejorGlobal(int filaActual, int[,] pob)
        {
            for (int a = 0; a < cantidadPuntos + 2; a++)
            {
                mejorSolucionGlobal[a] = pob[filaActual, a];
            }
        }

        private void BuscarMejorSolución(int[,] pob)
        {
            bool mejoró = false;
            for (int fila = 0; fila < cantPoblación; fila++)
            {
                if (pob[fila, cantidadPuntos + 1] < mejorSolucionGlobal[cantidadPuntos + 1])
                {
                    GuardarMejorGlobal(fila, pob);
                    mejoró = true;
                    búsquedasSinMejora = 0;
                }
            }
            if (mejoró == false)
            {
                búsquedasSinMejora++;
            }
        }
#endregion

        #region Cruzamiento
        private void ProcesoCruzamiento(int[,] pobContraria, int[,] pob)
        {
            if (probCruzamiento >= 0 && probCruzamiento <= 100)
            {
                int prob = rand.Next(1, 100);
                if (prob <= probCruzamiento)
                {
                    int[] valoresS1yS2 = ObtenerS1yS2();
                    TwoPointCrossover(true, 1, valoresS1yS2, pob, pobContraria);
                    TwoPointCrossover(false, -1, valoresS1yS2, pob, pobContraria);
                    seHizoCruzamiento = true;
                }
                else
                {
                    seHizoCruzamiento = false;
                }
            }
            else
            {
                seHizoCruzamiento = false;
            }
        }

        private int[] ObtenerS1yS2()
        {
            int S1 = rand.Next(1, cantidadPuntos - 3);
            int S2 = rand.Next(S1 + 1, cantidadPuntos);
            int temp = 0;
            if (S1 > S2)
            {
                temp = S1;
                S1 = S2; 
                S2 = temp;
            }

            int[] valoresS1yS2 = { S1, S2 };
            return valoresS1yS2;
        }

        private void TwoPointCrossover(bool esPar, int intercambio, int[] valoresS1yS2, int[,] pob, int[,] pobContraria)
        {
            int parImpar = esPar ? 0 : 1;
            //Dispatcher.Invoke(new Action(() =>
            //{
            //    S1yS2Cruzamiento.Text = ("S1 y S2: " + valoresS1yS2[0].ToString() + ", " + valoresS1yS2[1].ToString());
            //}));           

            //LLenar desde el padre 1 al hijo
            for (int a = parImpar; a < cantPoblación; a += 2)
            {
                //LLenar desde el padre 1 al hijo
                for (int b = 1; b < cantidadPuntos + 2; b++)
                {
                    pob[a, 0] = 0;
                    if (b <= valoresS1yS2[0] || b >= valoresS1yS2[1])
                    {
                        //Pasar todos los elementos del padre 1 dentro de los rangos, al hijo
                        pob[a, b] = pobContraria[a, b];                                             
                    }                   
                }

                //Verificar que no sea duplicado                
                int fila = a + intercambio;
                int columna = 1;

                for(int b = 2; b < cantidadPuntos; b++)
                {
                    if ( !(b <= valoresS1yS2[0] || b >= valoresS1yS2[1]) )
                    {
                        bool bandera = false;
                        while (bandera == false)
                        {
                            int valorActual = pobContraria[fila, columna];

                            if (!EsDuplicado(a, valorActual, valoresS1yS2, pobContraria))
                            {
                                pob[a, b] = valorActual;
                                bandera = true;
                            }
                            else
                            {
                                columna++;
                            }
                        }
                        columna++;
                    }
                }                
            }           
        }

        private bool EsDuplicado(int a, int valor, int[] valoresS1yS2, int[,] pobContraria)
        {
            for (int col = 1; col < cantidadPuntos; col++)
            {
                if (pobContraria[a, col] != 0)
                {
                    if (col <= valoresS1yS2[0] || col >= valoresS1yS2[1])
                    {
                        if (pobContraria[a, col] == valor)
                        {
                            return true;
                        }
                    }                        
                }
            }
            return false;
        }
        #endregion

        #region Mutación
        private bool ProcesoMutación()
        {
            if (probMutación >= 1 && probMutación <= 100)
            {
                int probabilidad = rand.Next(1, 100);
                if (probabilidad <= probMutación)
                {
                    return true;
                }
            }
            return false;
        }

        private void MutaciónSwap(int[,] pob)
        {
            for (int fila = 0; fila < cantPoblación; fila++)
            {
                int[] N1yN2 = ObtenerS1yS2();
                
                int aux = pob[fila, N1yN2[1]];
                pob[fila, N1yN2[1]] = pob[fila, N1yN2[0]];
                pob[fila, N1yN2[0]] = aux;         
            }           
        }
        #endregion

        #region Interfaz

        private void RedibujarPuntos()
        {
            for (int i = 0; i < coordenadas.Count; i++)
            {              
                Dispatcher.Invoke(new Action(() =>
                {
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };

                    Canvas.SetLeft(ellipse, coordenadas[i].Item1);
                    Canvas.SetTop(ellipse, coordenadas[i].Item2);

                    if (i == 0)
                    {
                        ellipse.Fill = Brushes.Red;
                    }
                    else
                    {
                        ellipse.Fill = Brushes.Black;
                    }

                    canvas.Children.Add(ellipse);
                }));
            }
        }

        private void DibujarRuta()
        {
            for(int i = 0; i < mejorSolucionGlobal.Length - 2; i++)
            {   
                // Agregar la línea al Canvas
                Dispatcher.Invoke(new Action(() =>
                {
                    Line línea = new Line();
                    línea.X1 = coordenadas[mejorSolucionGlobal[i]].Item1 + 5;
                    línea.Y1 = coordenadas[mejorSolucionGlobal[i]].Item2 + 5;
                    línea.X2 = coordenadas[mejorSolucionGlobal[i + 1]].Item1 + 5;
                    línea.Y2 = coordenadas[mejorSolucionGlobal[i + 1]].Item2 + 5;
                    línea.Stroke = Brushes.Green;
                    línea.StrokeThickness = 2;

                    canvas.Children.Add(línea);
                }));
            }
        }

        private void MostrarRutasPob(int[,] pob, ListBox lb, int cantX, int cantY)
        {
            //lb.Items.Clear();
            for (int i = 0; i < cantX; i++)
            {
                matrizMostrar = "";
                for (int j = 0; j < cantY +2; j++)
                {
                    matrizMostrar += (pob[i, j] + ",    ");
                }
                matrizMostrar += "\n";

                Dispatcher.Invoke(new Action(() =>
                {
                    lb.Items.Add(matrizMostrar);
                }));
            }
        }

        private void MostrarMejor()
        {
            mejor = "";
            for (int i = 0; i < cantidadPuntos; i++)
            {
                mejor += mejorSolucionGlobal[i] + ", ";
            }
            mejor += mejorSolucionGlobal[cantidadPuntos] + " ";
            mejor += "= " + mejorSolucionGlobal[cantidadPuntos + 1];

            // Utiliza Dispatcher para actualizar el TextBox en el hilo de la IU principal
            Dispatcher.Invoke(() =>
            {
                tBoxSolución.Text = mejor;
            });
        }

        private void btnMostrar_MouseEnter(object sender, MouseEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var widthAnimation = new DoubleAnimation() { To = 155, Duration = TimeSpan.FromSeconds(0.25) };
            var heightAnimation = new DoubleAnimation() { To = 40, Duration = TimeSpan.FromSeconds(0.25) };            

            btn.BeginAnimation(Button.WidthProperty, widthAnimation);
            btn.BeginAnimation(Button.HeightProperty, heightAnimation);
        }

        private void btnMostrar_MouseLeave(object sender, MouseEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var widthAnimation = new DoubleAnimation() { To = 150, Duration = TimeSpan.FromSeconds(0.25) };
            var heightAnimation = new DoubleAnimation() { To = 35, Duration = TimeSpan.FromSeconds(0.25) };

            btn.BeginAnimation(Button.WidthProperty, widthAnimation);
            btn.BeginAnimation(Button.HeightProperty, heightAnimation);
        }
        #endregion
    }
}
