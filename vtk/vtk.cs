using geometry_library;
using matrix_library;

namespace vtk
{
    public enum PRIMITIVE_TYPE
    {
        UNKNOWN,
        VERTICES,
        LINES,
        POLYGONS,
        TRIANGLE_STRIPS
    }
    public enum NORMAL_TYPE
    {
        UNKNOWN,
        POINT_DATA,
        CELL_DATA
    }
    public struct primative
    {
        public int num_of_points;
        public int[] indices;
    }

    /// <summary>
    /// Object for manipulating VTK data
    /// </summary>
    public class vtk_object
    {
        /// <summary>
        /// The number of points in the VTK object
        /// </summary>
        public int num_points
        {
            get
            {
                if (points == null)
                    return 0;
                return points.Length;
            }
        }
        /// <summary>
        /// The array of points locations
        /// </summary>
        public point3d[] points = null;
        /// <summary>
        /// The primative type of the VTK object
        /// </summary>
        public PRIMITIVE_TYPE primitive_type = PRIMITIVE_TYPE.UNKNOWN;
        /// <summary>
        /// The number of primatives in the VTK object
        /// </summary>
        public int num_primatives
        {
            get
            {
                if (primatives == null)
                    return 0;
                return primatives.Length;
            }
        }
        /// <summary>
        /// The primatives array
        /// Each priative will contain the number of points in that primative, and the indices into the 'points' array where that point is
        /// </summary>
        public primative[] primatives = null;
        /// <summary>
        /// The type of normals for this VTK object
        /// </summary>
        public NORMAL_TYPE normal_type = NORMAL_TYPE.UNKNOWN;
        /// <summary>
        ///  The number of normals in this VTK object
        /// </summary>
        public int num_normals
        {
            get
            {
                if (normals == null)
                    return 0;
                return normals.Length;
            }
        }
        /// <summary>
        /// The array of normals in matrix format (3x1)
        /// </summary>
        public matrix[] normals = null;


        /// <summary>
        /// Gets a point and the corresponding normal
        /// </summary>
        /// <param name="index">Index into the point (and normal) array</param>
        /// <returns>A point and normal pair</returns>
        public point_and_normal_pair_3d get_point_and_normal_pair_3d(int index)
        {
            if (index < 0 || index > points.Length)
                throw new System.ArgumentException("Index out of range!");

            point_and_normal_pair_3d result = new point_and_normal_pair_3d();
            result.x = points[index].x;
            result.y = points[index].y;
            result.z = points[index].z;
            result.normal_x = normals[index][0, 0];
            result.normal_y = normals[index][1, 0];
            result.normal_z = normals[index][2, 0];

            return result;
        }


        /// <summary>
        /// Creates an empty VTK object
        /// </summary>
        public vtk_object()
        {
        }
        /// <summary>
        /// Creates a VTK object that is a deep copy of the inputted object
        /// </summary>
        /// <param name="vtk_object_to_copy">The VTK object to copy</param>
        public vtk_object(vtk_object vtk_object_to_copy)
        {
            // Copy the points
            points = new point3d[vtk_object_to_copy.num_points];
            for (int i = 0; i < vtk_object_to_copy.num_points; ++i)
                points[i] = new point3d(vtk_object_to_copy.points[i]);

            // Copy the primatives
            primitive_type = vtk_object_to_copy.primitive_type;
            primatives = new primative[vtk_object_to_copy.num_primatives];
            for (int i = 0; i < vtk_object_to_copy.num_primatives; ++i)
            {
                primatives[i].num_of_points = vtk_object_to_copy.primatives[i].num_of_points;
                primatives[i].indices = new int[primatives[i].num_of_points];
                for (int j = 0; j < primatives[i].num_of_points; ++j)
                    primatives[i].indices[j] = vtk_object_to_copy.primatives[i].indices[j];
            }

            // Copy the normals
            normal_type = vtk_object_to_copy.normal_type;
            normals = new matrix[vtk_object_to_copy.num_normals];
            for (int i = 0; i < vtk_object_to_copy.num_normals; ++i)
                normals[i] = new matrix(vtk_object_to_copy.normals[i]);
        }
        /// <summary>
        /// Creates a VTK object and reads the VTK file indicated
        /// </summary>
        /// <param name="file_name">The path and filename to read</param>
        public vtk_object(string file_name) : this()
        {
            open(file_name);
        }
        /// <summary>
        /// Reads a VTK file
        /// Note:  this overwrites the current data in this object
        /// </summary>
        /// <param name="file_name">The path and filename to read</param>
        public virtual void open(string file_name)
        {
            // Helper anonymous functions
            System.Func<System.IO.BinaryReader, string> read_line = br =>
            {
                System.Text.StringBuilder result = new System.Text.StringBuilder();

                do
                {
                    result.Clear();
                    char c = br.ReadChar();
                    while (c != '\n')
                    {
                        result.Append(c);
                        c = br.ReadChar();
                    }
                    if (result.ToString().EndsWith("\r"))  // If the file has Windows line endings 
                        result.Remove(result.Length - 1, 1);  // eliminate the carriage return
                } while (result.ToString() == "");

                return result.ToString();
            };
            System.Func<System.IO.BinaryReader, string> read_token = br =>
            {
                System.Text.StringBuilder result = new System.Text.StringBuilder();

                do
                {
                    char c = br.ReadChar();
                    while (c != ' ' && c != '\n')
                    {
                        result.Append(c);
                        c = br.ReadChar();
                    }
                    if (result.ToString().EndsWith("\r"))  // If the file has Windows line endings 
                        result.Remove(result.Length - 1, 1);  // eliminate the carriage return
                } while (result.ToString() == "");

                return result.ToString();
            };

            System.IO.BinaryReader file = new System.IO.BinaryReader(System.IO.File.Open(file_name, System.IO.FileMode.Open));
            string text;

            try
            {
                // Check if the file identifier is correct
                text = read_line(file);
                if (text != "# vtk DataFile Version 3.0")
                    throw new System.FormatException("The VTK file header (version 3.0) was not found!");
                text = read_line(file);  // Skip the header

                // Determine if the VTK file is in ASCII or BINARY format
                bool binary = false;
                text = read_line(file);
                if (text == "ASCII")
                    binary = false;
                else if (text == "BINARY")
                    binary = true;
                else
                    throw new System.FormatException("The VTK file format could not be determined (ASCII or binary)!");

                // Check for the polygonal dataset structure identifier
                text = read_line(file);
                if (text != "DATASET POLYDATA")
                    throw new System.FormatException("The VTK file is in an unexpected format (couldn't find polygonal dataset)!");

                // Check for the data points identifer
                text = read_token(file);
                if (text != "POINTS")
                    throw new System.FormatException("The VTK file is in an unexpected format (couldn't find points identifier)!");

                // Read the number of data points
                text = read_token(file);
                int num_points = System.Convert.ToInt32(text);
                if (num_points <= 0)
                    throw new System.FormatException("The number of data points could not be determined!");

                // Check for the float type for the data points
                text = read_line(file);
                if (text != "float")
                    throw new System.FormatException("The VTK file is in an unexpected format (data points aren't floating point)!");

                // Read the points data
                points = new point3d[num_points];
                for (int i = 0; i < num_points; ++i)
                {
                    points[i] = new point3d();

                    if (binary == false)  // ASCII encoded values
                    {
                        text = read_token(file);
                        points[i].x = float.Parse(text);
                        text = read_token(file);
                        points[i].y = float.Parse(text);
                        text = read_token(file);
                        points[i].z = float.Parse(text);
                    }
                    else  // Binary encoded values
                    {
                        byte temp;
                        byte[] bytes;

                        bytes = file.ReadBytes(4);
                        // Swap endianness
                        temp = bytes[0];
                        bytes[0] = bytes[3];
                        bytes[3] = temp;
                        temp = bytes[1];
                        bytes[1] = bytes[2];
                        bytes[2] = temp;
                        points[i].x = System.BitConverter.ToSingle(bytes, 0);

                        bytes = file.ReadBytes(4);
                        // Swap endianness
                        temp = bytes[0];
                        bytes[0] = bytes[3];
                        bytes[3] = temp;
                        temp = bytes[1];
                        bytes[1] = bytes[2];
                        bytes[2] = temp;
                        points[i].y = System.BitConverter.ToSingle(bytes, 0);

                        bytes = file.ReadBytes(4);
                        // Swap endianness
                        temp = bytes[0];
                        bytes[0] = bytes[3];
                        bytes[3] = temp;
                        temp = bytes[1];
                        bytes[1] = bytes[2];
                        bytes[2] = temp;
                        points[i].z = System.BitConverter.ToSingle(bytes, 0);
                    }
                }

                // Read the polygonal primitive identifier
                text = read_token(file);
                if (text == "VERTICES")
                    primitive_type = PRIMITIVE_TYPE.VERTICES;
                else if (text == "LINES")
                    primitive_type = PRIMITIVE_TYPE.LINES;
                else if (text == "POLYGONS")
                    primitive_type = PRIMITIVE_TYPE.POLYGONS;
                else if (text == "TRIANGLE_STRIPS")
                    primitive_type = PRIMITIVE_TYPE.TRIANGLE_STRIPS;
                else
                {
                    clear();
                    throw new System.FormatException("The VTK file is in an unexpected format (unrecognized polygonal primitive type)!");
                }

                // Read the number of primitives
                text = read_token(file);
                int num_primatives = System.Convert.ToInt32(text);
                if (num_primatives <= 0)
                    throw new System.FormatException("The number of polygonal primitives could not be determined!");

                // Read and ignore the number of integers used in the primitives list
                // (Our dynamic array will handle each primitive separately)
                text = read_line(file);

                // Read the polygonal primitives
                primatives = new primative[num_primatives];
                for (int i = 0; i < num_primatives; ++i)
                {
                    // Read the size of the current primitive
                    int prim_size = 0;
                    if (binary == false)  // ASCII encoded values
                    {
                        text = read_token(file);
                        prim_size = int.Parse(text);
                    }
                    else  // Binary encoded values
                    {
                        byte temp;
                        byte[] bytes;

                        bytes = file.ReadBytes(4);
                        // Swap endianness
                        temp = bytes[0];
                        bytes[0] = bytes[3];
                        bytes[3] = temp;
                        temp = bytes[1];
                        bytes[1] = bytes[2];
                        bytes[2] = temp;
                        prim_size = System.BitConverter.ToInt32(bytes, 0);
                    }

                    if (prim_size <= 0)
                        throw new System.FormatException("A primitive's size could not be determined!");
                    primatives[i].indices = new int[prim_size];
                    primatives[i].num_of_points = prim_size;  // Don't count the size itself

                    if (binary == false)  // ASCII encoded values
                    {
                        for (int j = 0; j < prim_size; ++j)
                        {
                            text = read_token(file);
                            primatives[i].indices[j] = int.Parse(text);
                        }
                    }
                    else  // Binary encoded values
                    {
                        for (int j = 0; j < prim_size; ++j)
                        {
                            byte temp;
                            byte[] bytes;

                            bytes = file.ReadBytes(4);
                            // Swap endianness
                            temp = bytes[0];
                            bytes[0] = bytes[3];
                            bytes[3] = temp;
                            temp = bytes[1];
                            bytes[1] = bytes[2];
                            bytes[2] = temp;
                            primatives[i].indices[j] = System.BitConverter.ToInt32(bytes, 0);
                        }
                    }
                }

                // Search for a normal vector section among the dataset attributes.
                // Rather than parse every possible VTK data section, we're just going to look
                // for the headers POINT_DATA, CELL_DATA, or NORMALS following a newline
                NORMAL_TYPE attribute_type = NORMAL_TYPE.UNKNOWN;
                int num_normals = 0;
                while (true)
                {
                    text = read_token(file);
                    if (text == "POINT_DATA")  // Following dataset attributes are per-point
                    {
                        text = read_token(file);
                        num_normals = System.Convert.ToInt32(text);
                        if (num_normals != num_points)
                            throw new System.FormatException("Unexpected size of point dataset attributes!");
                        else
                            attribute_type = NORMAL_TYPE.POINT_DATA;  // We're in the point dataset attributes section
                    }
                    else if (text == "CELL_DATA")  // Following dataset attributes are per-primitive
                    {
                        text = read_token(file);
                        num_normals = System.Convert.ToInt32(text);
                        if (num_normals != num_primatives)
                            throw new System.FormatException("Unexpected size of cell (primitive) dataset attribute!.");
                        else
                            attribute_type = NORMAL_TYPE.CELL_DATA;  // We're in the point dataset attributes section
                    }
                    else if (text == "NORMALS")
                    {
                        if (attribute_type != NORMAL_TYPE.UNKNOWN)  // We're in a valid section
                        {
                            // Read and ignore the normal section's dataName (we only support one set of normals)
                            text = read_token(file);

                            // Check if the normals vectors are specified in floating point format
                            text = read_token(file);
                            if (text != "float")
                                throw new System.FormatException("Normal vectors not read because they are not floating point!");

                            // Set the type of normals
                            normal_type = attribute_type;

                            // Read the normal vectors
                            normals = new matrix[num_normals];
                            for (int i = 0; i < num_normals; ++i)
                            {
                                normals[i] = new matrix(3, 1);

                                if (binary == false)  // ASCII encoded values
                                {
                                    text = read_token(file);
                                    (normals[i])[0, 0] = float.Parse(text);
                                    text = read_token(file);
                                    (normals[i])[1, 0] = float.Parse(text);
                                    text = read_token(file);
                                    (normals[i])[2, 0] = float.Parse(text);
                                }
                                else  // Binary encoded values
                                {
                                    byte temp;
                                    byte[] bytes;

                                    bytes = file.ReadBytes(4);
                                    // Swap endianness
                                    temp = bytes[0];
                                    bytes[0] = bytes[3];
                                    bytes[3] = temp;
                                    temp = bytes[1];
                                    bytes[1] = bytes[2];
                                    bytes[2] = temp;
                                    (normals[i])[0, 0] = System.BitConverter.ToSingle(bytes, 0);

                                    bytes = file.ReadBytes(4);
                                    // Swap endianness
                                    temp = bytes[0];
                                    bytes[0] = bytes[3];
                                    bytes[3] = temp;
                                    temp = bytes[1];
                                    bytes[1] = bytes[2];
                                    bytes[2] = temp;
                                    (normals[i])[1, 0] = System.BitConverter.ToSingle(bytes, 0);

                                    bytes = file.ReadBytes(4);
                                    // Swap endianness
                                    temp = bytes[0];
                                    bytes[0] = bytes[3];
                                    bytes[3] = temp;
                                    temp = bytes[1];
                                    bytes[1] = bytes[2];
                                    bytes[2] = temp;
                                    (normals[i])[2, 0] = System.BitConverter.ToSingle(bytes, 0);
                                }
                            }

                            // Success!
                            break;
                        }
                    }
                }
            }
            catch (System.IO.EndOfStreamException)
            {
                throw new System.FormatException("The VTK file ended unexpectedly!");
            }
        }
        /// <summary>
        /// Sets the VTK object back to default/empty values
        /// </summary>
        public void clear()
        {
            points = null;
            primitive_type = PRIMITIVE_TYPE.UNKNOWN;
            primatives = null;
            normal_type = NORMAL_TYPE.UNKNOWN;
            normals = null;
        }
    }
}