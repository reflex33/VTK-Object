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
    public struct point
    {
        public float x;
        public float y;
        public float z;
    }
    public struct normal
    {
        public float x;
        public float y;
        public float z;
    }
    public struct primative
    {
        public int num_of_points;
        public int[] indices;
    }

    public class vtk_object
    {
        public int num_points
        {
            get
            {
                if (points == null)
                    return 0;
                return points.Length;
            }
        }
        public point[] points = null;
        public PRIMITIVE_TYPE primitive_type = PRIMITIVE_TYPE.UNKNOWN;
        public int num_primatives
        {
            get
            {
                if (primatives == null)
                    return 0;
                return primatives.Length;
            }
        }
        public primative[] primatives = null;
        bool normals_per_vertex = false;
        public int num_normals
        {
            get
            {
                if (normals == null)
                    return 0;
                return normals.Length;
            }
        }
        public normal[] normals = null;

        public vtk_object()
        {
        }
        public vtk_object(string file_name)
        {
            open(file_name);
        }
        public void open(string file_name)
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
                        result.Remove(result.Length - 1, 1);  // Eliminate the carriage return
                } while (result.ToString() == "");

                return result.ToString();
            };
            System.Func<System.IO.BinaryReader, string> read_to_delimiter = br =>
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
                        result.Remove(result.Length - 1, 1);  // Eliminate the carriage return
                } while (result.ToString() == "");

                return result.ToString();
            };

            System.IO.BinaryReader file = new System.IO.BinaryReader(System.IO.File.Open(file_name, System.IO.FileMode.Open));
            string text;

            //// Check if the file identifier is correct
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
            text = read_to_delimiter(file);
            if (text != "POINTS")
                throw new System.FormatException("The VTK file is in an unexpected format (couldn't find points identifier)!");

            // Read the number of data points
            text = read_to_delimiter(file);
            int num_points = System.Convert.ToInt32(text);
            if (num_points <= 0)
                throw new System.FormatException("The number of data points could not be determined!");

            // Check for the float type for the data points
            text = read_line(file);
            if (text != "float")
                throw new System.FormatException("The VTK file is in an unexpected format (data points aren't floating point)!");

            // Read the points data
            points = new point[num_points];
            for (int i = 0; i < num_points; ++i)
            {
                if (binary == false)  // ASCII encoded values
                {
                    text = read_to_delimiter(file);
                    points[i].x = float.Parse(text);
                    text = read_to_delimiter(file);
                    points[i].y = float.Parse(text);
                    text = read_to_delimiter(file);
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
            text = read_to_delimiter(file);
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
            text = read_to_delimiter(file);
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
                    text = read_to_delimiter(file);
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
                        text = read_to_delimiter(file);
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
            text = read_line(file);  // Skip new-line
        }
        public void clear()
        {
            points = null;
            primitive_type = PRIMITIVE_TYPE.UNKNOWN;
            primatives = null;
            normals_per_vertex = false;
            normals = null;
        }
    }
}