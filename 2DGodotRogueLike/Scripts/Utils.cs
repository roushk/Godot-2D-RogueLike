using Godot;
using System;
using Godot.Collections;

//Effectively a namespace of Utils
static public class Utils 
{
    //harder than I thought :)
    //static public Array<TypeName> ParseJsonFileIntoArrayOf<TypeName>(string JsonFilePath, Func<object[],TypeName> ctor) where TypeName : new()
    //{
    //    //Read json file into text
    //    Godot.File files = new Godot.File();
    //    files.Open(JsonFilePath, Godot.File.ModeFlags.Read);
    //    string jsonText = files.GetAsText();
    //    
    //    //Construct dict of stuff
    //    Godot.Collections.Dictionary<string,Godot.Collections.Array<TypeName>> ParsedData = Godot.JSON.Parse(jsonText).Result as Godot.Collections.Dictionary<string,Godot.Collections.Array<TypeName>>;
    //
    //    //Parse data
    //
    //    foreach (object data in ParsedData)
    //    {
    //        dataInstance = data as TypeName
    //    }
    //    
    //    Array<TypeName> objects = new Array<TypeName>();
    //    object[] ctorParams = new object[]{};
    //    TypeName newType = ctor(ctorParams);
    //    return objects;
    //}
}
