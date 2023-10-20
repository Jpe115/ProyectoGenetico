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
    enum TipoCruzamiento
    {
        TPX,
        OPX
    }
    
    enum TipoMutación
    {
        Swap,
        Switch2
    }

    public partial class MainWindow : Window
    {
        private List<(int, int)> coordenadas = new List<(int, int)>();
        private bool primerPunto = true;
        private string matrizMostrar = "";
        private int[,] distancias = new int[1, 1];
        private int cantidadPuntos;
        private int cantPoblación;
        private int[,] Población = new int[1, 1];
        private Random rand = new Random();
        private int[] mejorSolucionGlobal = new int[1];
        private int[,] Población2 = new int[1, 1];
        private string mejor = "";
        private int probCruzamiento;
        private int probMutación;
        private int nCiclos;
        private int ciclo;
        private bool esPob1Actual = true;
        private int búsquedasSinMejora;
        private double intentosSinMejora;
        private bool seAñadióPunto;
        private int ejecucionesRepetidas;
        private int cantPoblaciónActual;
        private bool cantPoblaciónCambió;
        private TipoCruzamiento cruzamiento;
        private TipoMutación mutación;
        private int cantidadDePuntosEntre2;

        private Dictionary<int, int> poblacionesChicas = new Dictionary<int, int> {
            { 1, 1 },
            { 2, 1 },
            { 3, 1 },
            { 4, 6 },
            { 5, 24 },
            { 6, 60 },
        };

        public int CantPoblación { get => cantPoblación ; set { 
                if (value >= 10 && value <= 2000) {
                cantPoblación = value;
                }
                else
                {
                    throw new Exception("La cantidad de población debe estar entre 10 y 2000. Se usará el valor predeterminado de 500.");
                }
            }
        }

        public int ProbabilidadCruzamiento
        {
            get => probCruzamiento; 
            set {
                if (value >= 0 && value <= 100)
                {
                    probCruzamiento = value;
                }
                else
                {
                    throw new Exception("La probabilidad de cruzamiento debe estar entre 0 y 100. Se usará el valor predeterminado de 90.");
                }
            }
        }

        public int ProbabilidadMutación { get => probMutación; set { 
                if(value >= 0 && value <= 100)
                {
                    probMutación = value;
                }
                else
                {
                    throw new Exception("La probabilidad de mutación debe estar entre 0 y 100. Se usará el valor predeterminado de 20.");
                }
            } 
        }

        public int NumeroCiclos { get => nCiclos; set {
                if (value >= 0 && value <= 10000)
                {
                    nCiclos = value;
                }
                else
                {
                    throw new Exception("El número de ciclos debe estar entre 0 y 10,000. Se usará el valor predeterminado de 100.");
                }
            } 
        }

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

            if(cantidadPuntos > 5)
            {
                btnEjecutar.IsEnabled = true;
            }
            btnReiniciar.IsEnabled = true;
            seAñadióPunto = true;
            ejecucionesRepetidas = 0;
        }

        private async void Ejecutar(object sender, RoutedEventArgs e)
        {
            DateTime antes = DateTime.Now;
            Cursor = Cursors.Wait;
            canvas.IsEnabled = false;
            btnEjecutar.IsEnabled = false;
            btnCancelar.IsEnabled = true;
            ciclo = 0;

            if (seAñadióPunto)
            {
                await Task.Run(ObtenerDistancias);                
            }

            #region Obtención de variables del usuario
            //elegir cantidad de población para cuando son menos de 7 ciudades
            // y establecer 100 de población por default
            try
            {
                CantPoblación = Convert.ToInt32(nPoblacion.Text);
            }
            catch (Exception ex)
            {
                cantPoblación = 500;
                MessageBox.Show(ex.Message, "Cantidad fuera de rango", MessageBoxButton.OK, MessageBoxImage.Warning);
                nPoblacion.Text = 500.ToString();
            }
            if (cantidadPuntos < 7)
            {
                cantPoblación = poblacionesChicas[cantidadPuntos];
            }
            //Comprobar si el usuario cambió la cantidad de la población
            if (cantPoblaciónActual != CantPoblación)
            {
                cantPoblaciónCambió = true;
                cantPoblaciónActual = CantPoblación;
            }
            else
            {
                cantPoblaciónCambió = false;
            }

            try
            {
                ProbabilidadCruzamiento = Convert.ToInt32(ProbCruzamiento.Text);
            }
            catch (Exception ex)
            {
                probCruzamiento = 90;
                MessageBox.Show(ex.Message, "Cantidad fuera de rango", MessageBoxButton.OK, MessageBoxImage.Warning);
                ProbCruzamiento.Text = 90.ToString();
            }

            try
            {
                ProbabilidadMutación = Convert.ToInt32(ProbMutación.Text);
            }
            catch (Exception ex)
            {
                probMutación = 20;
                MessageBox.Show(ex.Message, "Cantidad fuera de rango", MessageBoxButton.OK, MessageBoxImage.Warning);
                ProbMutación.Text = 20.ToString();
            }

            try
            {
                NumeroCiclos = Convert.ToInt32(Ciclos.Text);
            }
            catch (Exception ex)
            {
                nCiclos = 100;
                MessageBox.Show(ex.Message, "Cantidad fuera de rango", MessageBoxButton.OK, MessageBoxImage.Warning);
                Ciclos.Text = 100.ToString();
            }

            if(CruzamientoElegido.SelectedIndex == 0)
            {
                cruzamiento = TipoCruzamiento.TPX;
            }
            else
            {
                cruzamiento = TipoCruzamiento.OPX;
            }

            if (MutaciónElegida.SelectedIndex == 0)
            {
                mutación = TipoMutación.Swap;
            }
            else
            {
                mutación = TipoMutación.Switch2;
            }

            //Redondear en caso de ser necesario para la mutación switch2
            cantidadDePuntosEntre2 = (int)Math.Ceiling((double)cantidadPuntos / 2);
            cantidadDePuntosEntre2--;
            #endregion

            if (ejecucionesRepetidas == 0)
            {                
                mejorSolucionGlobal = new int[cantidadPuntos + 2];
                mejorSolucionGlobal[cantidadPuntos + 1] = 999999999;
                //intentosSinMejora = Math.Pow(cantidadPuntos, 2) * 0.33;
            }
            
            if (cantPoblaciónCambió || seAñadióPunto)
            {
                Población = new int[cantPoblación, cantidadPuntos + 2];
                Población2 = new int[cantPoblación, cantidadPuntos + 2];

                await Task.Run(() => {
                    InicializarPoblación(Población);
                    GenerarPobInicial(Población);
                    CalcularAptitud(Población);

                    InicializarPoblación(Población2);
                    GenerarPobInicial(Población2);
                    CalcularAptitud(Población2);
                });
            }            

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
                        if (ProcesoCruzamiento(Población, Población2))
                        {
                            CalcularAptitud(Población2);
                            BuscarMejorSolución(Población2);
                            esPob1Actual = !esPob1Actual;
                        }
                    }
                    else
                    {
                        if (ProcesoCruzamiento(Población2, Población))
                        {
                            CalcularAptitud(Población);
                            BuscarMejorSolución(Población);
                            esPob1Actual = !esPob1Actual;
                        }
                    }

                    if (esPob1Actual)
                    {
                        if (ProcesoMutación(Población))
                        {
                            CalcularAptitud(Población);
                            BuscarMejorSolución(Población);
                            esPob1Actual = !esPob1Actual;
                        }                                                
                    }
                    else
                    {
                        MostrarRutasPob(Población2, LB1, 50, cantidadPuntos);
                        if (ProcesoMutación(Población2))
                        {                           
                            CalcularAptitud(Población2);
                            BuscarMejorSolución(Población2);
                            esPob1Actual = !esPob1Actual;
                            MostrarRutasPob(Población2, LB2, 50, cantidadPuntos);
                        }
                    }
                });             

                //Cambios al canvas
                if(ciclo % 10 == 0)
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
            } while (ciclo < nCiclos);
            
            ejecucionesRepetidas++;
            DateTime después = DateTime.Now;
            TimeSpan total = después - antes;
            Tiempo.Text = total.ToString();
            canvas.IsEnabled = true;
            btnEjecutar.IsEnabled = true;
            btnCancelar.IsEnabled = false;
            seAñadióPunto = false;
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
        private void InicializarPoblación(int[,] pob)
        {
            for(int a = 0; a < cantPoblación; a++)
            {
                for(int b = 0; b < cantidadPuntos; b++)
                {
                    pob[a, b] = b;
                }
                pob[a, cantidadPuntos] = 0;
            }
        }

        private void GenerarPobInicial(int[,] pob)
        {
            for (int a = 0; a < cantPoblación; a++)
            {
                for (int b = 1; b < cantidadPuntos; b++)
                {
                    int temp = pob[a, b];
                    int random = rand.Next(1, cantidadPuntos - 1);
                    pob[a, b] = pob[a, random];
                    pob[a, random] = temp;
                    
                }
                pob[a, cantidadPuntos] = 0;
                pob[a, cantidadPuntos +1] = 0;
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
        private bool ProcesoCruzamiento(int[,] pobContraria, int[,] pob)
        {
            if (probCruzamiento >= 0 && probCruzamiento <= 100)
            {
                int prob = rand.Next(1, 100);
                if (prob <= probCruzamiento)
                {
                    if(cruzamiento == TipoCruzamiento.TPX)
                    {
                        int[] valoresS1yS2 = ObtenerS1yS2();
                        TwoPointCrossover(true, 1, valoresS1yS2, pob, pobContraria);
                        TwoPointCrossover(false, -1, valoresS1yS2, pob, pobContraria);
                        return true;
                    }
                    else
                    {
                        int S1 = rand.Next(3, cantidadPuntos - 3);
                        OnePointCrossover(true, 1, S1, pob, pobContraria);
                        OnePointCrossover(false, -1, S1, pob, pobContraria);
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void OnePointCrossover(bool esPar, int intercambio, int punto, int[,] pob, int[,] pobContraria)
        {
            int parImpar = esPar ? 0 : 1;

            //LLenar desde el padre 1 al hijo
            for (int a = parImpar; a < cantPoblación; a += 2)
            {
                //LLenar desde el padre 1 al hijo
                for (int b = 1; b <= punto; b++)
                {
                    //Pasar todos los elementos del padre 1 dentro de los rangos, al hijo
                    pob[a, b] = pobContraria[a, b];
                }

                //Verificar que no sea duplicado                
                int fila = a + intercambio;
                int columna = 1;

                for (int b = 2; b < cantidadPuntos; b++)
                {
                    if (!(b <= punto))
                    {
                        bool bandera = false;
                        while (bandera == false && columna < cantidadPuntos)
                        {
                            int valorActual = pobContraria[fila, columna];

                            if (!EsDuplicado(a, valorActual, punto, pobContraria))
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

        private bool EsDuplicado(int a, int valor, int punto, int[,] pobContraria)
        {
            for (int col = 1; col <= punto; col++)
            {
                if (pobContraria[a, col] != 0)
                {
                    if (pobContraria[a, col] == valor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int[] ObtenerS1yS2()
        {
            int S1 = rand.Next(1, cantidadPuntos - 3);
            int S2 = rand.Next(S1 + 1, cantidadPuntos);

            int[] valoresS1yS2 = { S1, S2 };
            return valoresS1yS2;
        }

        private void TwoPointCrossover(bool esPar, int intercambio, int[] valoresS1yS2, int[,] pob, int[,] pobContraria)
        {
            int parImpar = esPar ? 0 : 1;         

            //LLenar desde el padre 1 al hijo
            for (int a = parImpar; a < cantPoblación; a += 2)
            {
                //LLenar desde el padre 1 al hijo
                for (int b = 1; b < cantidadPuntos + 2; b++)
                {
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
                        while (bandera == false && columna < cantidadPuntos)
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
        private bool ProcesoMutación(int[,] pob)
        {
            if (probMutación >= 1 && probMutación <= 100)
            {
                int probabilidad = rand.Next(1, 100);
                if (probabilidad <= probMutación)
                {
                    if (mutación == TipoMutación.Swap)
                    {
                        MutaciónSwap(pob);
                        return true;
                    }
                    else
                    {
                        MutaciónSwitch2(pob);
                        return true;
                    }
                }
                else
                {
                    return false;
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

        private void MutaciónSwitch2(int[,] pob)
        {           
            for (int fila = 0; fila < cantPoblación; fila++)
            {
                int punto = rand.Next(1, (cantidadPuntos / 2) + 1);
                int aux = pob[fila, punto];
                pob[fila, punto] = pob[fila, punto + cantidadDePuntosEntre2];
                pob[fila, punto + cantidadDePuntosEntre2] = aux;
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

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            ciclo = nCiclos;
        }
        #endregion

        private void btnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            btnEjecutar.IsEnabled = false;
            tBoxGen.Text = "0";
            tBoxSolución.Text = "Por encontrar";
            mejorSolucionGlobal = new int[1];
            mejorSolucionGlobal[0] = 999999999;
            Población = new int[1, 1];
            Población2 = new int[1, 1];
            cantidadPuntos = 0;
            coordenadas.Clear();
            btnReiniciar.IsEnabled = false;
        }
    }
}
