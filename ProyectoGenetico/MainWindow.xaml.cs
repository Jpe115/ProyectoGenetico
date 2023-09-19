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

        private Dictionary<int, int> poblacionesChicas = new Dictionary<int, int> {
            { 1, 1 },
            { 2, 1 },
            { 3, 1 },
            { 4, 4 },
            { 5, 12 },
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
            canvas.IsEnabled = false;
            await Task.Run(obtenerDistancias);

            //elegir cantidad de población para cuando son menos de 7 ciudades
            // y establecer 100 de población por default
            try
            {
                cantPoblación = Convert.ToInt32(nPoblacion.Text);
            }
            catch (Exception)
            {
                cantPoblación = 100;
            }
            if(cantidadPuntos < 7)
            {
                cantPoblación = poblacionesChicas[cantidadPuntos];
            }
            Población = new int[cantPoblación, cantidadPuntos + 2];

            await Task.Run(() => {
                InicializarPoblación();
                GenerarPobInicial();
                CalcularAptitud();
                ProcesoSelección();
            });

            //Cambios a los listBox
            listBox.Items.Clear();
            listBox2.Items.Clear();
            listBoxDistancias.Items.Clear();
            await Task.Run(() =>
            {
                MostrarRutasPob(Población, listBox, cantPoblación, cantidadPuntos);
                MostrarRutasPob(Población2, listBox2, cantPoblación, cantidadPuntos);
                MostrarRutasPob(distancias, listBoxDistancias, cantidadPuntos, cantidadPuntos - 2);
            });

            //Cambios al canvas
            canvas.Children.Clear();
            Thread th1 = new Thread(() =>
            {
                RedibujarPuntos();
                DibujarRuta();
            });
            th1.Start();
            Thread th2 = new Thread(MostrarMejor);            
            th2.Start();

            canvas.IsEnabled = true;
        }

        private void obtenerDistancias()
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

        private void CalcularAptitud()
        {            
            for (int a = 0; a < cantPoblación; a++)
            {
                int aptitud = 0;
                for (int b = 1; b < cantidadPuntos; b++)
                {
                    aptitud += distancias[Población[a, b], Población[a, b +1]];
                }
                Población[a, cantidadPuntos +1] = aptitud;
            }
        }

        private void ProcesoSelección()
        {
            Población2 = new int[cantPoblación, cantidadPuntos + 2];
            mejorSolucionGlobal = new int[cantidadPuntos + 2];

            int mejorSolucionBinaria = 0;
            int temp;

            for(int a = 0;a < cantPoblación; a++)
            {
                int r = rand.Next(0,cantPoblación-1);
                if (Población[a, cantidadPuntos + 1] < Población[r, cantidadPuntos + 1])
                {
                    temp = Población[a, cantidadPuntos + 1];
                    CopiarSolucion(a, a);                    
                }
                else
                {
                    temp = Población[r, cantidadPuntos + 1];
                    CopiarSolucion(r, a);
                }

                //guardar mejor solución global
                if(a == 0)
                {
                    mejorSolucionBinaria = temp;
                    GuardarMejorGlobal(a);
                }
                if (temp < mejorSolucionBinaria)
                {
                    GuardarMejorGlobal(a);
                }

                mejorSolucionBinaria = temp;
            }
        }

        private void CopiarSolucion(int filaGanadora, int filaActual)
        {
            for(int a = 0; a < cantidadPuntos+2; a++)
            {
                Población2[filaActual, a] = Población[filaGanadora, a];
            }
        }

        private void GuardarMejorGlobal(int filaActual)
        {
            for (int a = 0; a < cantidadPuntos + 2; a++)
            {
                mejorSolucionGlobal[a] = Población[filaActual, a];
            }
        }

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
            var widthAnimation = new DoubleAnimation() { To = 155, Duration = TimeSpan.FromSeconds(0.25) };
            var heightAnimation = new DoubleAnimation() { To = 40, Duration = TimeSpan.FromSeconds(0.25) };

            btn.BeginAnimation(Button.WidthProperty, widthAnimation);
            btn.BeginAnimation(Button.HeightProperty, heightAnimation);
        }

        private void btnMostrar_MouseLeave(object sender, MouseEventArgs e)
        {
            var btn = sender as Button;
            var widthAnimation = new DoubleAnimation() { To = 150, Duration = TimeSpan.FromSeconds(0.25) };
            var heightAnimation = new DoubleAnimation() { To = 35, Duration = TimeSpan.FromSeconds(0.25) };

            btn.BeginAnimation(Button.WidthProperty, widthAnimation);
            btn.BeginAnimation(Button.HeightProperty, heightAnimation);
        }
    }
}
