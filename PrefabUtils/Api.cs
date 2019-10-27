namespace StompyNZ
{
  class Api : IModApi
  {
    public void InitMod()
    {

    }

    public static void Log(string message = "") => SingletonMonoBehaviour<SdtdConsole>.Instance.Output(message);
  }
}
