using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.ConditionalDraw;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;


namespace mobielecommunicatie.ViewModel
{
    public partial

        class MainPageViewModel : ObservableObject
    {

        // stappen
        [ObservableProperty]
        private int steps;

        // laatst opgeslagen waarde voor history
        DateTime lastdateSaved;

        // doel
        [ObservableProperty]
        static int goal = 100;

        // motivatie tekst
        [ObservableProperty]
        String motivation;

        // percentage voor de "pie chart/gauge"
        private ObservableValue _steps_percent = new ObservableValue(0);

        // 10 laatste waarden van de accelerometer Z as
        private List<double> values_accelerometer = new List<double>();

        // de waarde en layout voor de pie chart
        public IEnumerable<ISeries> Series2 { get; set; }

        // de waarden en layout voor de grafiek history
        public ISeries[] Series { get; set; } =
        {
            new LineSeries<ObservablePoint>
            {
                Values = new ObservableCollection<ObservablePoint>{},
                Stroke = new LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint(new[]{ new SKColor(255, 255, 255), new SKColor(75, 232, 229)}) { StrokeThickness = 2 },
                GeometryStroke =  new LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint(new[]{ new SKColor(255, 255, 255), new SKColor(55, 212, 209)}) { StrokeThickness = 2 },
                GeometrySize = 0,
                LineSmoothness = 0.2
            },

        };

        // de x-as layout voor history
        public Axis[] XAxes { get; set; } =
        {
            new Axis
            {
                Labeler = value => $"{(int)value/100}:{((value%100)*60/100).ToString("00")}",
            
            }
        };

        // de y-as layout voor history
        public Axis[] YAxes { get; set; } =
        {
            new Axis
            {
                MinLimit = 0,
                MinStep = 10,
                MaxLimit = goal,
                SeparatorsPaint = new SolidColorPaint
                {
                    Color = SKColors.Gray,
                    StrokeThickness = 1,
                },
                SubseparatorsCount = 3
            }
        };

        



        public MainPageViewModel()
        {
            // initiaiseer stappen op 0
            steps = 0;

            // zet de eerste waarde voor de history grafiek
            lastdateSaved = DateTime.Now;
            var values = (ObservableCollection<ObservablePoint>)Series[0].Values!;
            values.Add(new ObservablePoint((DateTime.Now.Minute*100/60) + DateTime.Now.Hour * 100, Steps));

            // voeg het percentage toe voor de gauge
            Series2 = GaugeGenerator.BuildSolidGauge(
                new GaugeItem(GaugeItem.Background, series =>
                {
                    series.InnerRadius = 75;
                    series.Fill = new SolidColorPaint(new SKColor(100, 181, 246, 90));
                   
                }),
                  
           

                new GaugeItem(_steps_percent, series =>
                {
                    series.Fill = new SolidColorPaint(SKColors.AliceBlue);
                    series.DataLabelsSize = 40;
                    series.DataLabelsPaint = new SolidColorPaint(SKColors.AliceBlue);
                    series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
                    series.InnerRadius = 75;
                    series.DataLabelsFormatter = percent => percent.Coordinate.PrimaryValue.ToString() + "%";
                    series.WithConditionalPaint(new SolidColorPaint(new SKColor(155, 176, 82))).When(point => point.Model?.Value >= 100);
                })
            );   
           
            // start de accelerometer
           ToggleAccelerometer();
        }


        private void ToggleAccelerometer()
        {

            if (Accelerometer.Default.IsSupported)
            {
                if (!Accelerometer.Default.IsMonitoring)
                {
                    // Turn on accelerometer
                    Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                    Accelerometer.Default.Start(SensorSpeed.UI);
                }

            }

        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            // voeg de nieuwe waarde toe aan de lijst
            values_accelerometer.Add(e.Reading.Acceleration.Y);

            
            if (values_accelerometer.Count > 10)
            {
                // de hoogste en laagste waarde worden vergelijken met de threshold om het as een stap te zien
                if ((values_accelerometer.Max() - values_accelerometer.Min()) > 1)
                {
                    Steps++;
                    // verwijder alle waarde omdat deze stap niet meer opnieuw geteld moet worden
                    values_accelerometer.RemoveRange(0, 10);
                    // kijk of de huidige stappen moeten worden opgeslagen, de waarde wordt maximaal elke minuut opgeslagen als er stappen bijkomen
                    var diffOfDates = DateTime.Now - lastdateSaved;
                    if (diffOfDates.Minutes >= 1 || diffOfDates.Hours > 0)
                    {
                        ObservablePoint data = new ObservablePoint((DateTime.Now.Minute*100/60) + DateTime.Now.Hour * 100, Steps);
                        var values = (ObservableCollection<ObservablePoint>)Series[0].Values!;
                        values.Add(data);
                        lastdateSaved = DateTime.Now;


                    }

                    // update het percentage in de pie chart
                    if (Steps * 100 / goal > 100)
                    {
                        _steps_percent.Value = 100;
                    } else
                    {
                        _steps_percent.Value = Steps * 100 / goal;
                    }
                    

                    // verander de motivatie aan de hand van het aantal stappen
                    double diffGoal = (double)Steps / (double)Goal;
                    if (diffGoal < 0.1)
                    {
                        Motivation = "Goed onderweg!";
                    }
                    else if (diffGoal < 0.5)
                    {
                        Motivation = "Goed bezig!";
                    }
                    else if (Steps >= Goal)
                    {
                        Motivation = "Gehaald!!";
                    }
                    else { Motivation = "Bijna daar!"; }

                }
                // verwijder de oudste waarde in de array
                values_accelerometer.RemoveAt(0);
            }


        }

    }
}
