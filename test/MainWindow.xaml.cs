using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace significance_camera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D(-0.5, 0, 0);
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(0.5, 0.5, 0.3);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(0, 0.5, 0);
            System.Windows.Media.Media3D.MeshGeometry3D triangleMesh = new System.Windows.Media.Media3D.MeshGeometry3D();
            triangleMesh.Positions.Add(point0);
            triangleMesh.Positions.Add(point1);
            triangleMesh.Positions.Add(point2);
            int n0 = 0;
            int n1 = 1;
            int n2 = 2;
            triangleMesh.TriangleIndices.Add(n0);
            triangleMesh.TriangleIndices.Add(n1);
            triangleMesh.TriangleIndices.Add(n2);
            System.Windows.Media.Media3D.Vector3D norm = new System.Windows.Media.Media3D.Vector3D(0, 0, 1);
            triangleMesh.Normals.Add(norm);
            triangleMesh.Normals.Add(norm);
            triangleMesh.Normals.Add(norm);
            System.Windows.Media.Media3D.Material frontMaterial = new System.Windows.Media.Media3D.DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            System.Windows.Media.Media3D.GeometryModel3D triangleModel = new System.Windows.Media.Media3D.GeometryModel3D(triangleMesh, frontMaterial);
            triangleModel.Transform = new System.Windows.Media.Media3D.Transform3DGroup();
            System.Windows.Media.Media3D.ModelVisual3D visualModel = new System.Windows.Media.Media3D.ModelVisual3D();
            visualModel.Content = triangleModel;

            System.Windows.Media.Media3D.DirectionalLight light_source = new System.Windows.Media.Media3D.DirectionalLight();
            light_source.Color = Colors.White;
            light_source.Direction = new System.Windows.Media.Media3D.Vector3D(1, 1, -1);
            System.Windows.Media.Media3D.ModelVisual3D visualModel_light = new System.Windows.Media.Media3D.ModelVisual3D();
            visualModel_light.Content = light_source;

            System.Windows.Media.Media3D.OrthographicCamera camera = new System.Windows.Media.Media3D.OrthographicCamera();
            camera.FarPlaneDistance = 10;
            camera.NearPlaneDistance = 1;
            camera.LookDirection = new System.Windows.Media.Media3D.Vector3D(0, 0, -1);
            camera.UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0);
            camera.Position = new System.Windows.Media.Media3D.Point3D(0, 0, 2);

            this.viewport.Children.Add(visualModel);
            viewport.Children.Add(visualModel_light);
            viewport.Camera = camera;

            vtk.vtk_object v = new vtk.vtk_object("cube.vtk");
            //vtk.vtk_object v = new vtk.vtk_object("kidney.vtk");
        }
    }
}
