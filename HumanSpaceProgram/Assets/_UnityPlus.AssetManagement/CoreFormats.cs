namespace UnityPlus.AssetManagement
{
    public static class CoreFormats
    {
        // --- Images ---
        public static readonly AssetFormat Png = new AssetFormat( "PNG" );
        public static readonly AssetFormat Jpg = new AssetFormat( "JPG" );
        public static readonly AssetFormat Tga = new AssetFormat( "TGA" );
        public static readonly AssetFormat Bmp = new AssetFormat( "BMP" );
        public static readonly AssetFormat Tiff = new AssetFormat( "TIFF" );
        public static readonly AssetFormat Exr = new AssetFormat( "EXR" );
        public static readonly AssetFormat Dds = new AssetFormat( "DDS" );

        // --- Models ---
        public static readonly AssetFormat Obj = new AssetFormat( "OBJ" );
        public static readonly AssetFormat Fbx = new AssetFormat( "FBX" );
        public static readonly AssetFormat Gltf = new AssetFormat( "GLTF" );
        public static readonly AssetFormat Glb = new AssetFormat( "GLB" );
        public static readonly AssetFormat Dae = new AssetFormat( "DAE" );

        // --- Audio ---
        public static readonly AssetFormat Wav = new AssetFormat( "WAV" );
        public static readonly AssetFormat Ogg = new AssetFormat( "OGG" );
        public static readonly AssetFormat Mp3 = new AssetFormat( "MP3" );
        public static readonly AssetFormat Aiff = new AssetFormat( "AIFF" );
        public static readonly AssetFormat Flac = new AssetFormat( "FLAC" );

        // --- Video ---
        public static readonly AssetFormat Mp4 = new AssetFormat( "MP4" );
        public static readonly AssetFormat Mov = new AssetFormat( "MOV" );
        public static readonly AssetFormat Webm = new AssetFormat( "WEBM" );

        // --- Data / Text ---
        public static readonly AssetFormat Json = new AssetFormat( "JSON" );
        public static readonly AssetFormat Xml = new AssetFormat( "XML" );
        public static readonly AssetFormat Yaml = new AssetFormat( "YAML" );
        public static readonly AssetFormat Csv = new AssetFormat( "CSV" );

        public static readonly AssetFormat Txt = new AssetFormat( "TXT" );
        public static readonly AssetFormat Bytes = new AssetFormat( "BYTES" ); // Generic binary
    }
}