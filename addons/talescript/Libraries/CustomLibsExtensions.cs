using Code.TaleScript.Libraries.Actor;
using Code.TaleScript.Libraries.Dialog;
using Code.TaleScript.Libraries.Storage;

namespace Lua.Libraries;

public static class CustomLibsExtensions
{
    public static void OpenDialogLibrary(this LuaState state, IStorage storage = null, IDialog dialog = null)
    {
        foreach (LuaFunction func in DialogLibrary.Instance.Functions)
        {
            state.Environment[func.Name]= func;
        }
        DialogLibrary.Instance.Initialize(storage ?? IStorage.Dummy, dialog ?? IDialog.Dummy);
    }
    
    public static void OpenActorLibrary(this LuaState state, IActorProvider actorProvider = null)
    {
        foreach (LuaFunction func in ActorLibrary.Instance.Functions)
        {
            state.Environment[func.Name] = func;
        }
        ActorLibrary.Instance.Initialize(actorProvider ?? IActorProvider.Dummy);
    }
    
    public static void OpenDebugLibrary(this LuaState state)
    {
        foreach (LuaFunction func in DebugLibrary.Instance.Functions)
        {
            state.Environment[func.Name] = func;
        }
    }
    
    public static void OpenAsyncLibrary(this LuaState state)
    {
        foreach (LuaFunction func in AsyncLibrary.Instance.Functions)
        {
            state.Environment[func.Name] = func;
        }
    }
    
    public static void OpenCustomLibs(this LuaState state)
    {
        state.OpenDialogLibrary();
        state.OpenActorLibrary();
        state.OpenDebugLibrary();
        state.OpenAsyncLibrary();
    }
}