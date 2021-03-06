﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using vtk;

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
            // Make the kidney object for the scene
            wpf_vtk_object kidney = new wpf_vtk_object("kidney.vtk");
            System.Windows.Media.Media3D.Material kidney_material = new System.Windows.Media.Media3D.DiffuseMaterial(new SolidColorBrush(Colors.Blue));
            System.Windows.Media.Media3D.GeometryModel3D kidney_geometry_model = new System.Windows.Media.Media3D.GeometryModel3D(kidney.triangle_mesh, kidney_material);
            System.Windows.Media.Media3D.ModelVisual3D kidney_visual_model = new System.Windows.Media.Media3D.ModelVisual3D();
            kidney_visual_model.Content = kidney_geometry_model;

            // Make the cude object for the scene
            wpf_vtk_object cube = new wpf_vtk_object("cube.vtk");
            System.Windows.Media.Media3D.Material cube_material = new System.Windows.Media.Media3D.DiffuseMaterial(new SolidColorBrush(Colors.Red));
            System.Windows.Media.Media3D.GeometryModel3D cube_geometry_model = new System.Windows.Media.Media3D.GeometryModel3D(cube.triangle_mesh, cube_material);
            System.Windows.Media.Media3D.ModelVisual3D cube_visual_model = new System.Windows.Media.Media3D.ModelVisual3D();
            cube_visual_model.Content = cube_geometry_model;

            // Make the light source for the scene
            System.Windows.Media.Media3D.DirectionalLight light_source = new System.Windows.Media.Media3D.DirectionalLight();
            light_source.Color = Colors.White;
            light_source.Direction = new System.Windows.Media.Media3D.Vector3D(1, -1, -1);
            System.Windows.Media.Media3D.ModelVisual3D visualModel_light = new System.Windows.Media.Media3D.ModelVisual3D();
            visualModel_light.Content = light_source;

            // Make the camera for the scene
            System.Windows.Media.Media3D.PerspectiveCamera camera = new System.Windows.Media.Media3D.PerspectiveCamera();
            camera.FarPlaneDistance = 400;
            camera.NearPlaneDistance = 1;
            camera.FieldOfView = 48;
            camera.LookDirection = new System.Windows.Media.Media3D.Vector3D(0, 0, -1);
            camera.UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0);
            camera.Position = new System.Windows.Media.Media3D.Point3D(0, 0, 300);

            // Put the model, light, and camera in the viewport
            viewport.Children.Add(kidney_visual_model);
            viewport.Children.Add(cube_visual_model);
            viewport.Children.Add(visualModel_light);
            viewport.Camera = camera;

            // Moving the objects... moves kidney to the right and rotates it -90 degrees about the Z axis, cube moves to the left and rotates 45 about the y axis
            System.Windows.Media.Media3D.Matrix3D matrix = new System.Windows.Media.Media3D.Matrix3D(0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1, 0, 50, 0, 0, 1);
            System.Windows.Media.Media3D.Transform3D trans = new System.Windows.Media.Media3D.MatrixTransform3D(matrix);
            kidney_visual_model.Transform = trans;
            matrix = new System.Windows.Media.Media3D.Matrix3D(1, 0, -1, 0, 0, 1, 0, 0, 1, 0, 1, 0, -50, 0, 0, 1);
            trans = new System.Windows.Media.Media3D.MatrixTransform3D(matrix);
            cube_visual_model.Transform = trans;
        }
    }
}
