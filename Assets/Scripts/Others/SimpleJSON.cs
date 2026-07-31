/* * * * *
     * A simple JSON Parser / builder
     * ------------------------------
     * 
     * It mainly has been written as a simple JSON parser. It can build a JSON string
     * from the node-tree, or generate a node tree from any valid JSON string.
     * 
     * If you want to use compression when saving to file / stream / B64 you have to include
     * SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ ) in your project and
     * define "USE_SharpZipLib" at the top of the file
     * 
     * Written by Bunny83 
     * 2012-06-09
     * 
     * Features / attributes:
     * - provides strongly typed node classes and lists / dictionaries
     * - provides easy access to class members / array items / data values
     * - the parser ignores data types. Each value is a string.
     * - only double quotes (") are used for quoting strings.
     * - values and names are not restricted to quoted strings. They simply add up and are trimmed.
     * - There are only 3 types: arrays(JSONArray), objects(JSONClass) and values(JSONData)
     * - provides "casting" properties to easily convert to / from those types:
     *   int / float / double / bool
     * - provides a common interface for each node so no explicit casting is required.
     * - the parser try to avoid errors, but if malformed JSON is parsed the result is undefined
     * 
     * 
     * 2012-12-17 Update:
     * - Added internal JSONLazyCreator class which simplifies the construction of a JSON tree
     *   Now you can simple reference any item that doesn't exist yet and it will return a JSONLazyCreator
     *   The class determines the required type by it's further use, creates the type and removes itself.
     * - Added binary serialization / deserialization.
     * - Added support for BZip2 zipped binary format. Requires the SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ )
     *   The usage of the SharpZipLib library can be disabled by removing or commenting out the USE_SharpZipLib define at the top
     * - The serializer uses different types when it comes to store the values. Since my data values
     *   are all of type string, the serializer will "try" which format fits best. The order is: int, float, double, bool, string.
     *   It's not the most efficient way but for a moderate amount of data it should work on all platforms.
     * 
     * 2015-07-25 Update: (Paolo Abela)
     * - Added key enumerator for getting the keys of the json file
     * 
     * * * * */

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Proporciona utilidades para manipular y analizar datos JSON de forma sencilla.
    /// Permite crear, leer y modificar estructuras JSON en C#.
    /// </summary>
    namespace SimpleJSON
    {
        public enum JSONBinaryTag
        {
            Array = 1,
            Class = 2,
            Value = 3,
            IntValue = 4,
            DoubleValue = 5,
            BoolValue = 6,
            FloatValue = 7,
        }

        public class JSONNode
        {
            #region common interface
            /// <summary>
            /// Agrega un elemento al nodo JSON.
            /// </summary>
            /// <param name="aKey">Clave del elemento.</param>
            /// <param name="aItem">Valor a agregar.</param>
            public virtual void Add(string aKey, JSONNode aItem) { }
            /// <summary>
            /// Obtiene o establece un elemento del nodo JSON por índice.
            /// </summary>
            public virtual JSONNode this[int aIndex] { get { return null; } set { } }
            /// <summary>
            /// Obtiene o establece un elemento del nodo JSON por clave.
            /// </summary>
            public virtual JSONNode this[string aKey] { get { return null; } set { } }
            /// <summary>
            /// Obtiene o establece el valor del nodo JSON.
            /// </summary>
            public virtual string Value { get { return ""; } set { } }
            /// <summary>
            /// Obtiene la cantidad de elementos en el nodo JSON.
            /// </summary>
            public virtual int Count { get { return 0; } }

            /// <summary>
            /// Obtiene una colección de las claves en el nodo JSON.
            /// </summary>
            public virtual IEnumerable<string> Keys { get { yield break; } }

            /// <summary>
            /// Agrega un elemento al nodo JSON.
            /// </summary>
            /// <param name="aItem">Valor a agregar.</param>
            public virtual void Add(JSONNode aItem)
            {
                Add("", aItem);
            }

            /// <summary>
            /// Elimina un elemento del nodo JSON por clave.
            /// </summary>
            /// <param name="aKey">Clave del elemento a eliminar.</param>
            /// <returns>El nodo JSON eliminado.</returns>
            public virtual JSONNode Remove(string aKey) { return null; }
            /// <summary>
            /// Elimina un elemento del nodo JSON por índice.
            /// </summary>
            /// <param name="aIndex">Índice del elemento a eliminar.</param>
            /// <returns>El nodo JSON eliminado.</returns>
            public virtual JSONNode Remove(int aIndex) { return null; }
            /// <summary>
            /// Elimina un nodo JSON hijo.
            /// </summary>
            /// <param name="aNode">Nodo JSON a eliminar.</param>
            /// <returns>El nodo JSON eliminado.</returns>
            public virtual JSONNode Remove(JSONNode aNode) { return aNode; }

            /// <summary>
            /// Obtiene una colección de los nodos hijos del nodo JSON.
            /// </summary>
            public virtual IEnumerable<JSONNode> Childs { get { yield break; } }
            /// <summary>
            /// Obtiene una colección de pares clave-valor de los nodos hijos del nodo JSON.
            /// </summary>
            public virtual IEnumerable<KeyValuePair<string, JSONNode>> ChildsWithKeys { get { yield break; } }

            /// <summary>
            /// Obtiene todos los nodos hijos, incluidos los hijos de los hijos (recursivo).
            /// </summary>
            public IEnumerable<JSONNode> DeepChilds
            {
                get
                {
                    foreach (var C in Childs)
                        foreach (var D in C.DeepChilds)
                            yield return D;
                }
            }

            public override string ToString()
            {
                return "JSONNode";
            }
            /// <summary>
            /// Devuelve una representación en cadena del nodo JSON con un prefijo dado.
            /// </summary>
            /// <param name="aPrefix">El prefijo a aplicar.</param>
            /// <returns>Representación en cadena del nodo JSON.</returns>
            public virtual string ToString(string aPrefix)
            {
                return "JSONNode";
            }

            #endregion common interface

            #region typecasting properties
            /// <summary>
            /// Obtiene o establece el valor como un entero.
            /// </summary>
            public virtual int AsInt
            {
                get
                {
                    int v = 0;
                    if (int.TryParse(Value, out v))
                        return v;
                    return 0;
                }
                set
                {
                    Value = value.ToString();
                }
            }
            /// <summary>
            /// Obtiene o establece el valor como un número de punto flotante.
            /// </summary>
            public virtual float AsFloat
            {
                get
                {
                    float v = 0.0f;
                    if (float.TryParse(Value, out v))
                        return v;
                    return 0.0f;
                }
                set
                {
                    Value = value.ToString();
                }
            }
            /// <summary>
            /// Obtiene o establece el valor como un número de doble precisión.
            /// </summary>
            public virtual double AsDouble
            {
                get
                {
                    double v = 0.0;
                    if (double.TryParse(Value, out v))
                        return v;
                    return 0.0;
                }
                set
                {
                    Value = value.ToString();
                }
            }
            /// <summary>
            /// Obtiene o establece el valor como un booleano.
            /// </summary>
            public virtual bool AsBool
            {
                get
                {
                    bool v = false;
                    if (bool.TryParse(Value, out v))
                        return v;
                    return !string.IsNullOrEmpty(Value);
                }
                set
                {
                    Value = (value) ? "true" : "false";
                }
            }
            /// <summary>
            /// Convierte el nodo JSON a un arreglo JSON.
            /// </summary>
            public virtual JSONArray AsArray
            {
                get
                {
                    return this as JSONArray;
                }
            }
            /// <summary>
            /// Convierte el nodo JSON a un objeto JSON.
            /// </summary>
            public virtual JSONClass AsObject
            {
                get
                {
                    return this as JSONClass;
                }
            }


            #endregion typecasting properties

            #region operators
            /// <summary>
            /// Convierte implícitamente una cadena en un nodo JSON.
            /// </summary>
            /// <param name="s">La cadena a convertir.</param>
            public static implicit operator JSONNode(string s)
            {
                return new JSONData(s);
            }
            /// <summary>
            /// Convierte implícitamente un nodo JSON en una cadena.
            /// </summary>
            /// <param name="d">El nodo JSON a convertir.</param>
            public static implicit operator string(JSONNode d)
            {
                return (d == null) ? null : d.Value;
            }
            /// <summary>
            /// Compara dos nodos JSON para igualdad.
            /// </summary>
            /// <param name="a">El primer nodo JSON.</param>
            /// <param name="b">El segundo nodo JSON.</param>
            public static bool operator ==(JSONNode a, object b)
            {
                if (b == null && a is JSONLazyCreator)
                    return true;
                return System.Object.ReferenceEquals(a, b);
            }

            /// <summary>
            /// Compara dos nodos JSON para desigualdad.
            /// </summary>
            /// <param name="a">El primer nodo JSON.</param>
            /// <param name="b">El segundo nodo JSON.</param>
            public static bool operator !=(JSONNode a, object b)
            {
                return !(a == b);
            }
            public override bool Equals(object obj)
            {
                return System.Object.ReferenceEquals(this, obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }


            #endregion operators

            internal static string Escape(string aText)
            {
                string result = "";
                foreach (char c in aText)
                {
                    switch (c)
                    {
                        case '\\': result += "\\\\"; break;
                        case '\"': result += "\\\""; break;
                        case '\n': result += "\\n"; break;
                        case '\r': result += "\\r"; break;
                        case '\t': result += "\\t"; break;
                        case '\b': result += "\\b"; break;
                        case '\f': result += "\\f"; break;
                        default: result += c; break;
                    }
                }
                return result;
            }

            /// <summary>
            /// Analiza una cadena JSON y devuelve el nodo JSON raíz.
            /// </summary>
            /// <param name="aJSON">La cadena JSON a analizar.</param>
            /// <returns>El nodo JSON raíz.</returns>
            /// <exception cref="Exception">Si hay un error de análisis.</exception>
            public static JSONNode Parse(string aJSON)
            {
                Stack<JSONNode> stack = new Stack<JSONNode>();
                JSONNode ctx = null;
                int i = 0;
                string Token = "";
                string TokenName = "";
                bool QuoteMode = false;
                while (i < aJSON.Length)
                {
                    switch (aJSON[i])
                    {
                        case '{':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            stack.Push(new JSONClass());
                            if (ctx != null)
                            {
                                TokenName = TokenName.Trim();
                                if (ctx is JSONArray)
                                    ctx.Add(stack.Peek());
                                else if (TokenName != "")
                                    ctx.Add(TokenName, stack.Peek());
                            }
                            TokenName = "";
                            Token = "";
                            ctx = stack.Peek();
                            break;

                        case '[':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }

                            stack.Push(new JSONArray());
                            if (ctx != null)
                            {
                                TokenName = TokenName.Trim();
                                if (ctx is JSONArray)
                                    ctx.Add(stack.Peek());
                                else if (TokenName != "")
                                    ctx.Add(TokenName, stack.Peek());
                            }
                            TokenName = "";
                            Token = "";
                            ctx = stack.Peek();
                            break;

                        case '}':
                        case ']':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            if (stack.Count == 0)
                                throw new Exception("JSON Parse: Too many closing brackets");

                            stack.Pop();
                            if (Token != "")
                            {
                                TokenName = TokenName.Trim();
                                if (ctx is JSONArray)
                                    ctx.Add(Token);
                                else if (TokenName != "")
                                    ctx.Add(TokenName, Token);
                            }
                            TokenName = "";
                            Token = "";
                            if (stack.Count > 0)
                                ctx = stack.Peek();
                            break;

                        case ':':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            TokenName = Token;
                            Token = "";
                            break;

                        case '"':
                            QuoteMode ^= true;
                            break;

                        case ',':
                            if (QuoteMode)
                            {
                                Token += aJSON[i];
                                break;
                            }
                            if (Token != "")
                            {
                                if (ctx is JSONArray)
                                    ctx.Add(Token);
                                else if (TokenName != "")
                                    ctx.Add(TokenName, Token);
                            }
                            TokenName = "";
                            Token = "";
                            break;

                        case '\r':
                        case '\n':
                            break;

                        case ' ':
                        case '\t':
                            if (QuoteMode)
                                Token += aJSON[i];
                            break;

                        case '\\':
                            ++i;
                            if (QuoteMode)
                            {
                                char C = aJSON[i];
                                switch (C)
                                {
                                    case 't': Token += '\t'; break;
                                    case 'r': Token += '\r'; break;
                                    case 'n': Token += '\n'; break;
                                    case 'b': Token += '\b'; break;
                                    case 'f': Token += '\f'; break;
                                    case 'u':
                                        {
                                            string s = aJSON.Substring(i + 1, 4);
                                            Token += (char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
                                            i += 4;
                                            break;
                                        }
                                    default: Token += C; break;
                                }
                            }
                            break;

                        default:
                            Token += aJSON[i];
                            break;
                    }
                    ++i;
                }
                if (QuoteMode)
                {
                    throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
                }
                return ctx;
            }

            /// <summary>
            /// Serializa el nodo JSON en un flujo binario.
            /// </summary>
            /// <param name="aWriter">El escritor de flujo binario.</param>
            public virtual void Serialize(System.IO.BinaryWriter aWriter) { }

            /// <summary>
            /// Guarda el nodo JSON en un flujo.
            /// </summary>
            /// <param name="aData">El flujo donde guardar los datos JSON.</param>
            public void SaveToStream(System.IO.Stream aData)
            {
                var W = new System.IO.BinaryWriter(aData);
                Serialize(W);
            }

#if USE_SharpZipLib
            /// <summary>
            /// Guarda el nodo JSON en un flujo comprimido.
            /// </summary>
            /// <param name="aData">El flujo donde guardar los datos JSON comprimidos.</param>
            public void SaveToCompressedStream(System.IO.Stream aData)
            {
                using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
                {
                    gzipOut.IsStreamOwner = false;
                    SaveToStream(gzipOut);
                    gzipOut.Close();
                }
            }

            /// <summary>
            /// Guarda el nodo JSON en un archivo comprimido.
            /// </summary>
            /// <param name="aFileName">El nombre del archivo donde guardar los datos JSON comprimidos.</param>
            public void SaveToCompressedFile(string aFileName)
            {
#if USE_FileIO
                System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
                using (var F = System.IO.File.OpenWrite(aFileName))
                {
                    SaveToCompressedStream(F);
                }
#else
			throw new Exception("Can't use File IO stuff in webplayer");
#endif
            }
            /// <summary>
            /// Guarda el nodo JSON como una cadena Base64 comprimida.
            /// </summary>
            /// <returns>Cadena Base64 que representa los datos JSON comprimidos.</returns>
            public string SaveToCompressedBase64()
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    SaveToCompressedStream(stream);
                    stream.Position = 0;
                    return System.Convert.ToBase64String(stream.ToArray());
                }
            }

#else
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public void SaveToCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public string SaveToCompressedBase64()
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
#endif

            /// <summary>
            /// Guarda el nodo JSON en un archivo.
            /// </summary>
            /// <param name="aFileName">El nombre del archivo donde guardar los datos JSON.</param>
            public void SaveToFile(string aFileName)
            {
#if USE_FileIO
                System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
                using (var F = System.IO.File.OpenWrite(aFileName))
                {
                    SaveToStream(F);
                }
#else
			throw new Exception("Can't use File IO stuff in webplayer");
#endif
            }
            /// <summary>
            /// Guarda el nodo JSON como una cadena Base64.
            /// </summary>
            /// <returns>Cadena Base64 que representa los datos JSON.</returns>
            public string SaveToBase64()
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    SaveToStream(stream);
                    stream.Position = 0;
                    return System.Convert.ToBase64String(stream.ToArray());
                }
            }
            /// <summary>
            /// Deserializa un nodo JSON desde un flujo binario.
            /// </summary>
            /// <param name="aReader">El lector de flujo binario.</param>
            /// <returns>El nodo JSON deserializado.</returns>
            public static JSONNode Deserialize(System.IO.BinaryReader aReader)
            {
                JSONBinaryTag type = (JSONBinaryTag)aReader.ReadByte();
                switch (type)
                {
                    case JSONBinaryTag.Array:
                        {
                            int count = aReader.ReadInt32();
                            JSONArray tmp = new JSONArray();
                            for (int i = 0; i < count; i++)
                                tmp.Add(Deserialize(aReader));
                            return tmp;
                        }
                    case JSONBinaryTag.Class:
                        {
                            int count = aReader.ReadInt32();
                            JSONClass tmp = new JSONClass();
                            for (int i = 0; i < count; i++)
                            {
                                string key = aReader.ReadString();
                                var val = Deserialize(aReader);
                                tmp.Add(key, val);
                            }
                            return tmp;
                        }
                    case JSONBinaryTag.Value:
                        {
                            return new JSONData(aReader.ReadString());
                        }
                    case JSONBinaryTag.IntValue:
                        {
                            return new JSONData(aReader.ReadInt32());
                        }
                    case JSONBinaryTag.DoubleValue:
                        {
                            return new JSONData(aReader.ReadDouble());
                        }
                    case JSONBinaryTag.BoolValue:
                        {
                            return new JSONData(aReader.ReadBoolean());
                        }
                    case JSONBinaryTag.FloatValue:
                        {
                            return new JSONData(aReader.ReadSingle());
                        }

                    default:
                        {
                            throw new Exception("Error deserializing JSON. Unknown tag: " + type);
                        }
                }
            }

#if USE_SharpZipLib
            /// <summary>
            /// Carga un nodo JSON desde un flujo comprimido.
            /// </summary>
            /// <param name="aData">El flujo comprimido desde donde cargar los datos JSON.</param>
            /// <returns>El nodo JSON cargado.</returns>
            public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
            {
                var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
                return LoadFromStream(zin);
            }
            /// <summary>
            /// Carga un nodo JSON desde un archivo comprimido.
            /// </summary>
            /// <param name="aFileName">El nombre del archivo comprimido desde donde cargar los datos JSON.</param>
            /// <returns>El nodo JSON cargado.</returns>
            public static JSONNode LoadFromCompressedFile(string aFileName)
            {
#if USE_FileIO
                using (var F = System.IO.File.OpenRead(aFileName))
                {
                    return LoadFromCompressedStream(F);
                }
#else
			throw new Exception("Can't use File IO stuff in webplayer");
#endif
            }
            /// <summary>
            /// Carga un nodo JSON desde una cadena Base64 comprimida.
            /// </summary>
            /// <param name="aBase64">La cadena Base64 que representa los datos JSON comprimidos.</param>
            /// <returns>El nodo JSON cargado.</returns>
            public static JSONNode LoadFromCompressedBase64(string aBase64)
            {
                var tmp = System.Convert.FromBase64String(aBase64);
                var stream = new System.IO.MemoryStream(tmp);
                stream.Position = 0;
                return LoadFromCompressedStream(stream);
            }
#else
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
#endif

            /// <summary>
            /// Carga un nodo JSON desde un flujo.
            /// </summary>
            /// <param name="aData">El flujo desde donde cargar los datos JSON.</param>
            /// <returns>El nodo JSON cargado.</returns>
            public static JSONNode LoadFromStream(System.IO.Stream aData)
            {
                using (var R = new System.IO.BinaryReader(aData))
                {
                    return Deserialize(R);
                }
            }
            /// <summary>
            /// Carga un nodo JSON desde un archivo.
            /// </summary>
            /// <param name="aFileName">El nombre del archivo desde donde cargar los datos JSON.</param>
            /// <returns>El nodo JSON cargado.</returns>
            public static JSONNode LoadFromFile(string aFileName)
            {
#if USE_FileIO
                using (var F = System.IO.File.OpenRead(aFileName))
                {
                    return LoadFromStream(F);
                }
#else
			throw new Exception("Can't use File IO stuff in webplayer");
#endif
            }
            /// <summary>
            /// Carga un nodo JSON desde una cadena Base64.
            /// </summary>
            /// <param name="aBase64">La cadena Base64 que representa los datos JSON.</param>
            /// <returns>El nodo JSON cargado.</returns>
            public static JSONNode LoadFromBase64(string aBase64)
            {
                var tmp = System.Convert.FromBase64String(aBase64);
                var stream = new System.IO.MemoryStream(tmp);
                stream.Position = 0;
                return LoadFromStream(stream);
            }
        } // End of JSONNode

        public class JSONArray : JSONNode, IEnumerable
        {
            List<JSONNode> m_List = new List<JSONNode>();
            public override JSONNode this[int aIndex]
            {
                get
                {
                    if (aIndex < 0 || aIndex >= m_List.Count)
                        return new JSONLazyCreator(this);
                    return m_List[aIndex];
                }
                set
                {
                    if (aIndex < 0 || aIndex >= m_List.Count)
                        m_List.Add(value);
                    else
                        m_List[aIndex] = value;
                }
            }
            public override JSONNode this[string aKey]
            {
                get { return new JSONLazyCreator(this); }
                set { m_List.Add(value); }
            }
            public override int Count
            {
                get { return m_List.Count; }
            }
            public override void Add(string aKey, JSONNode aItem)
            {
                m_List.Add(aItem);
            }
            public override JSONNode Remove(int aIndex)
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    return null;
                JSONNode tmp = m_List[aIndex];
                m_List.RemoveAt(aIndex);
                return tmp;
            }
            public override JSONNode Remove(JSONNode aNode)
            {
                m_List.Remove(aNode);
                return aNode;
            }
            public override IEnumerable<JSONNode> Childs
            {
                get
                {
                    foreach (JSONNode N in m_List)
                        yield return N;
                }
            }
            public IEnumerator GetEnumerator()
            {
                foreach (JSONNode N in m_List)
                    yield return N;
            }
            public override string ToString()
            {
                string result = "[ ";
                foreach (JSONNode N in m_List)
                {
                    if (result.Length > 2)
                        result += ", ";
                    result += N.ToString();
                }
                result += " ]";
                return result;
            }
            public override string ToString(string aPrefix)
            {
                string result = "[ ";
                foreach (JSONNode N in m_List)
                {
                    if (result.Length > 3)
                        result += ", ";
                    result += "\n" + aPrefix + "   ";
                    result += N.ToString(aPrefix + "   ");
                }
                result += "\n" + aPrefix + "]";
                return result;
            }
            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                aWriter.Write((byte)JSONBinaryTag.Array);
                aWriter.Write(m_List.Count);
                for (int i = 0; i < m_List.Count; i++)
                {
                    m_List[i].Serialize(aWriter);
                }
            }
        } // End of JSONArray

        public class JSONClass : JSONNode, IEnumerable
        {
            Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

            //[REM] Aggiunta per poter leggere le chaivi di ogni JSON,rimuoverla se genera problemi
            public override IEnumerable<string> Keys
            {
                get
                {
                    foreach (var key in m_Dict.Keys)
                        yield return key;
                }
            }

            public override IEnumerable<KeyValuePair<string, JSONNode>> ChildsWithKeys
            {
                get
                {
                    foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                        yield return N;
                }
            }

            public override JSONNode this[string aKey]
            {
                get
                {
                    if (m_Dict.ContainsKey(aKey))
                        return m_Dict[aKey];
                    else
                        return new JSONLazyCreator(this, aKey);
                }
                set
                {
                    if (m_Dict.ContainsKey(aKey))
                        m_Dict[aKey] = value;
                    else
                        m_Dict.Add(aKey, value);
                }
            }
            public override JSONNode this[int aIndex]
            {
                get
                {
                    if (aIndex < 0 || aIndex >= m_Dict.Count)
                        return null;
                    return m_Dict.ElementAt(aIndex).Value;
                }
                set
                {
                    if (aIndex < 0 || aIndex >= m_Dict.Count)
                        return;
                    string key = m_Dict.ElementAt(aIndex).Key;
                    m_Dict[key] = value;
                }
            }
            public override int Count
            {
                get { return m_Dict.Count; }
            }


            public override void Add(string aKey, JSONNode aItem)
            {
                if (!string.IsNullOrEmpty(aKey))
                {
                    if (m_Dict.ContainsKey(aKey))
                        m_Dict[aKey] = aItem;
                    else
                        m_Dict.Add(aKey, aItem);
                }
                else
                    m_Dict.Add(Guid.NewGuid().ToString(), aItem);
            }

            /// <summary>
            /// Elimina un elemento del objeto JSON por clave.
            /// </summary>
            /// <param name="aKey">Clave del elemento a eliminar.</param>
            /// <returns>El nodo JSON eliminado.</returns>
            public override JSONNode Remove(string aKey)
            {
                if (!m_Dict.ContainsKey(aKey))
                    return null;
                JSONNode tmp = m_Dict[aKey];
                m_Dict.Remove(aKey);
                return tmp;
            }
            /// <summary>
            /// Elimina un elemento del objeto JSON por índice.
            /// </summary>
            /// <param name="aIndex">Índice del elemento a eliminar.</param>
            /// <returns>El nodo JSON eliminado.</returns>
            public override JSONNode Remove(int aIndex)
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return null;
                var item = m_Dict.ElementAt(aIndex);
                m_Dict.Remove(item.Key);
                return item.Value;
            }
            /// <summary>
            /// Elimina un nodo JSON hijo.
            /// </summary>
            /// <param name="aNode">Nodo JSON a eliminar.</param>
            /// <returns>El nodo JSON eliminado.</returns>
            public override JSONNode Remove(JSONNode aNode)
            {
                try
                {
                    var item = m_Dict.Where(k => k.Value == aNode).First();
                    m_Dict.Remove(item.Key);
                    return aNode;
                }
                catch
                {
                    return null;
                }
            }

            public override IEnumerable<JSONNode> Childs
            {
                get
                {
                    foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                        yield return N.Value;
                }
            }

            public IEnumerator GetEnumerator()
            {
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                    yield return N;
            }
            public override string ToString()
            {
                string result = "{";
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                {
                    if (result.Length > 2)
                        result += ", ";
                    result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
                }
                result += "}";
                return result;
            }
            public override string ToString(string aPrefix)
            {
                string result = "{ ";
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                {
                    if (result.Length > 3)
                        result += ", ";
                    result += "\n" + aPrefix + "   ";
                    result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix + "   ");
                }
                result += "\n" + aPrefix + "}";
                return result;
            }
            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                aWriter.Write((byte)JSONBinaryTag.Class);
                aWriter.Write(m_Dict.Count);
                foreach (string K in m_Dict.Keys)
                {
                    aWriter.Write(K);
                    m_Dict[K].Serialize(aWriter);
                }
            }
        } // End of JSONClass

        public class JSONData : JSONNode
        {
            string m_Data;
            public override string Value
            {
                get { return m_Data; }
                set { m_Data = value; }
            }
            public JSONData(string aData)
            {
                m_Data = aData;
            }
            public JSONData(float aData)
            {
                AsFloat = aData;
            }
            public JSONData(double aData)
            {
                AsDouble = aData;
            }
            public JSONData(bool aData)
            {
                AsBool = aData;
            }
            public JSONData(int aData)
            {
                AsInt = aData;
            }

            public override string ToString()
            {
                return "\"" + Escape(m_Data) + "\"";
            }
            public override string ToString(string aPrefix)
            {
                return "\"" + Escape(m_Data) + "\"";
            }
            public override void Serialize(System.IO.BinaryWriter aWriter)
            {
                var tmp = new JSONData("");

                tmp.AsInt = AsInt;
                if (tmp.m_Data == this.m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.IntValue);
                    aWriter.Write(AsInt);
                    return;
                }
                tmp.AsFloat = AsFloat;
                if (tmp.m_Data == this.m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.FloatValue);
                    aWriter.Write(AsFloat);
                    return;
                }
                tmp.AsDouble = AsDouble;
                if (tmp.m_Data == this.m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.DoubleValue);
                    aWriter.Write(AsDouble);
                    return;
                }

                tmp.AsBool = AsBool;
                if (tmp.m_Data == this.m_Data)
                {
                    aWriter.Write((byte)JSONBinaryTag.BoolValue);
                    aWriter.Write(AsBool);
                    return;
                }
                aWriter.Write((byte)JSONBinaryTag.Value);
                aWriter.Write(m_Data);
            }
        } // End of JSONData

        internal class JSONLazyCreator : JSONNode
        {
            JSONNode m_Node = null;
            string m_Key = null;

            public JSONLazyCreator(JSONNode aNode)
            {
                m_Node = aNode;
                m_Key = null;
            }
            public JSONLazyCreator(JSONNode aNode, string aKey)
            {
                m_Node = aNode;
                m_Key = aKey;
            }

            void Set(JSONNode aVal)
            {
                if (m_Key == null)
                {
                    m_Node.Add(aVal);
                }
                else
                {
                    m_Node.Add(m_Key, aVal);
                }
                m_Node = null; // Be GC friendly.
            }

            public override JSONNode this[int aIndex]
            {
                get
                {
                    return new JSONLazyCreator(this);
                }
                set
                {
                    var tmp = new JSONArray();
                    tmp.Add(value);
                    Set(tmp);
                }
            }

            public override JSONNode this[string aKey]
            {
                get
                {
                    return new JSONLazyCreator(this, aKey);
                }
                set
                {
                    var tmp = new JSONClass();
                    tmp.Add(aKey, value);
                    Set(tmp);
                }
            }
            public override void Add(JSONNode aItem)
            {
                var tmp = new JSONArray();
                tmp.Add(aItem);
                Set(tmp);
            }
            public override void Add(string aKey, JSONNode aItem)
            {
                var tmp = new JSONClass();
                tmp.Add(aKey, aItem);
                Set(tmp);
            }
            public static bool operator ==(JSONLazyCreator a, object b)
            {
                if (b == null)
                    return true;
                return System.Object.ReferenceEquals(a, b);
            }

            public static bool operator !=(JSONLazyCreator a, object b)
            {
                return !(a == b);
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return true;
                return System.Object.ReferenceEquals(this, obj);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return "";
            }
            public override string ToString(string aPrefix)
            {
                return "";
            }

            public override int AsInt
            {
                get
                {
                    JSONData tmp = new JSONData(0);
                    Set(tmp);
                    return 0;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }
            public override float AsFloat
            {
                get
                {
                    JSONData tmp = new JSONData(0.0f);
                    Set(tmp);
                    return 0.0f;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }
            public override double AsDouble
            {
                get
                {
                    JSONData tmp = new JSONData(0.0);
                    Set(tmp);
                    return 0.0;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }
            public override bool AsBool
            {
                get
                {
                    JSONData tmp = new JSONData(false);
                    Set(tmp);
                    return false;
                }
                set
                {
                    JSONData tmp = new JSONData(value);
                    Set(tmp);
                }
            }
            public override JSONArray AsArray
            {
                get
                {
                    JSONArray tmp = new JSONArray();
                    Set(tmp);
                    return tmp;
                }
            }
            public override JSONClass AsObject
            {
                get
                {
                    JSONClass tmp = new JSONClass();
                    Set(tmp);
                    return tmp;
                }
            }
        } // End of JSONLazyCreator

        public static class JSON
        {
            /// <summary>
            /// Analiza una cadena JSON y devuelve el nodo JSON raíz.
            /// </summary>
            /// <param name="aJSON">La cadena JSON a analizar.</param>
            /// <returns>El nodo JSON raíz.</returns>
            public static JSONNode Parse(string aJSON)
            {
                return JSONNode.Parse(aJSON);
            }
        }
    }