using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ARToolkit.Forms;
using Colors = System.Drawing.Color;

namespace ARTemplateApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ARPage : ContentPage
    {
        public ARPage()
        {
            InitializeComponent();
            Camera originCamera = new Camera(55.660806, 12.401681, 9.64, 0, 90, 0);
            arSceneView.OriginCamera = originCamera;
            
            //We'll set the origin of the scene centered on Mnt Everest so we can use that as the tie-point when we tap to place

        }

      
       
        private async void InitializeScene()
        {
            try { 

                //var webSceneUrl = new Uri("https://geoinfo-support.maps.arcgis.com/home/webscene/viewer.html?webscene=47435c44d2d8483d9489218eac5329dd");
                Uri serviceFeatureTable_Uri = new Uri("https://services.arcgis.com/LeHsdRPbeEIElrVR/arcgis/rest/services/globalSceneTrainees_WFL1/FeatureServer/1");
                ServiceFeatureTable featureTable = new ServiceFeatureTable(serviceFeatureTable_Uri);


                FeatureLayer featureLayer = new FeatureLayer(featureTable) {
                    RenderingMode = FeatureRenderingMode.Dynamic
                };  

                SimpleLineSymbol mySimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Black, 1);
                SimpleFillSymbol mysimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Blue, mySimpleLineSymbol);
                SimpleRenderer mySimpleRenderer = new SimpleRenderer(mysimpleFillSymbol);

                var scene = new Scene(Basemap.CreateTopographic());
                //scene = new Scene(Basemap.CreateTopographic());
                scene.BaseSurface.BackgroundGrid.IsVisible = false;
                //scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));
                scene.BaseSurface.NavigationConstraint = NavigationConstraint.None;
                
            
                RendererSceneProperties myRendererSceneProperties = mySimpleRenderer.SceneProperties;
                myRendererSceneProperties.ExtrusionMode = ExtrusionMode.AbsoluteHeight;
                myRendererSceneProperties.ExtrusionExpression = "[height]";
                featureLayer.Renderer = mySimpleRenderer;

                scene.OperationalLayers.Add(featureLayer);
                await scene.LoadAsync();
                arSceneView.Scene = scene;
                double widthScene = 1300;
                double widthReal = 1;

                arSceneView.TranslationFactor = widthScene / widthReal;
                //scene.OperationalLayers.Add(layer);
                arSceneView.ClippingDistance = 800;
                arSceneView.SetInitialTransformation(TransformationMatrix.Create(0, 0, 0, 1, 0, .5, 1.5));
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Failed to load scene", ex.Message, "OK");
                await Navigation.PopAsync();
            }
        }


        protected override void OnAppearing()
        {
            Status.Text = "Move your device in a circular motion to detect surfaces";
            arSceneView.StartTrackingAsync();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            arSceneView.StopTrackingAsync();
            base.OnDisappearing();
        }

        private void DoubleTap_ToPlace(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            if (arSceneView.SetInitialTransformation(e.Position))
            {
                if (arSceneView.Scene == null)
                {
                    arSceneView.RenderPlanes = false;
                    Status.Text = string.Empty;
                    InitializeScene();
                    arSceneView.InteractionOptions = new SceneViewInteractionOptions { IsEnabled = false};
                }
            }
        }

        private void PlanesDetectedChanged(object sender, bool planesDetected)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!planesDetected)
                    Status.Text = "Move your device in a circular motion to detect surfaces";
                else if (arSceneView.Scene == null)
                    Status.Text = "Double-tap a plane to place your scene";
                else
                    Status.Text = string.Empty;
            });
        }
    }
}
