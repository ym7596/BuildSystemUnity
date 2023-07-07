namespace dduR
{
    public class SpaceConfig
    {
        public static string WallTag = "Wall";

        public static string ItemTag = "Items";

        public static string GroundTag = "Ground";

        public static string savePref = "roominfo";

        public static string OnObjectLayer = "OnObject";

        public static int newObject = 9999;

       
    }

    public enum ItemCategoryType
    {
        Bed = 0,
        Desk,
        Chair,
        Prop,
        Wall
    }

   



    public enum ObjectPlacedType
    {
        Floor = 0,
        Putable,
        Prop,
        Wall
    }

    public enum CameraViewDir
    {
        Front = 0,
        Right,
        Behind,
        Left
    }
}