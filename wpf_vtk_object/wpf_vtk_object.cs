using System.Windows.Media.Media3D;

namespace vtk
{
    public class wpf_vtk_object : vtk_object
    {
        public MeshGeometry3D triangle_mesh
        {
            get
            {
                MeshGeometry3D mesh = new MeshGeometry3D();

                // Add the points
                for (int i = 0; i < num_points; ++i)
                {
                    System.Windows.Media.Media3D.Point3D point = new System.Windows.Media.Media3D.Point3D(points[i].x, points[i].y, points[i].z);
                    mesh.Positions.Add(point);
                }

                // Add the triangles
                for (int i = 0; i < num_primatives; ++i)
                {
                    if (primitive_type == PRIMITIVE_TYPE.TRIANGLE_STRIPS)
                    {
                        for (int j = 0; j < primatives[i].num_of_points - 2; ++j)
                        {
                            if (j % 2 == 0)  // even, so triangle is in proper direction
                            {
                                mesh.TriangleIndices.Add(primatives[i].indices[j]);
                                mesh.TriangleIndices.Add(primatives[i].indices[j + 1]);
                                mesh.TriangleIndices.Add(primatives[i].indices[j + 2]);
                            }
                            else  // odd, so triangle is backwards (thanks triangle strips)
                            {
                                mesh.TriangleIndices.Add(primatives[i].indices[j + 2]);
                                mesh.TriangleIndices.Add(primatives[i].indices[j + 1]);
                                mesh.TriangleIndices.Add(primatives[i].indices[j]);
                            }
                        }
                    }
                    else
                        throw new System.NotImplementedException("Can't create triangle mesh for the primative type of this VTK object!");
                }

                // Add the normals
                System.Windows.Media.Media3D.Vector3D norm;
                for (int i = 0; i < num_normals; ++i)
                {
                    if (normal_type == NORMAL_TYPE.POINT_DATA)
                    {
                        norm = new System.Windows.Media.Media3D.Vector3D(normals[i].x, normals[i].y, normals[i].z);
                        mesh.Normals.Add(norm);
                    }
                    else
                        throw new System.NotImplementedException("Can't create triangle mesh for the normal type of this VTK object!");
                }

                return mesh;
            }
        }

        public wpf_vtk_object() : base()
        {
        }
        public wpf_vtk_object(string file_name) : base(file_name)
        {
        }
    }
}
