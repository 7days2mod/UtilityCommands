using System.Collections.Generic;

namespace StompyNZ.Commands
{
  public class PrefabUtils : IConsoleCommand
  {
    public PrefabUtils()
    {
    }

    public string[] GetCommands() => new[] { "prefabutil", "util" };

    public string GetDescription() => "";

    public string GetHelp() => "";

    public bool IsExecuteOnClient => false;

    public int DefaultPermissionLevel => 0;


    public void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
      if (null == GameManager.Instance.World)
      {
        Api.Log("World isn't loaded.");

        return;
      }

      if (_senderInfo.RemoteClientInfo == null)
      {
        Api.Log("Remote client not found.");

        return;
      }

      if (!GameManager.Instance.World.Players.dict.ContainsKey(_senderInfo.RemoteClientInfo.entityId))
      {
        Api.Log("Player entity not found.");

        return;
      }

      var player = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];

      if (!player.IsSpawned())
      {
        Api.Log("Player entity not spawned.");

        return;
      }

      Process(_params, player);
    }

    private static void Process(List<string> _params, EntityPlayer player)
    {
      var tag = QuestTags.none;

      var decorator = GameManager.Instance.GetDynamicPrefabDecorator();
      var prefabs = decorator.GetDynamicPrefabs();

      if (_params.Count > 0 && _params[0].EqualsCaseInsensitive("list"))
      {
        foreach (var p in prefabs)
        {
          Api.Log($"{p.name} @ {p.boundingBoxPosition.x},{p.boundingBoxPosition.y},{p.boundingBoxPosition.z}");
        }

        return;
      }

      var prefab = decorator.GetClosestPOIToWorldPos(tag, player.position);

      if (_params.Count > 0 && _params[0].EqualsCaseInsensitive("reset"))
      {
        if (prefab.CheckForAnyPlayerHome(GameManager.Instance.World) != GameUtils.EPlayerHomeType.None)
        {
          Api.Log("Can't reset a prefab that has been claimed in the past.");

          return;
        }

        Api.Log($"Resetting prefab @{prefab.boundingBoxPosition}");

        prefab.Reset(GameManager.Instance.World, tag);

        return;
      }

      Api.Log($"Name: {prefab.name}");
      Api.Log($"Id: {prefab.id}");
      Api.Log($"Loc: {prefab.boundingBoxPosition}");
      Api.Log($"Size: {prefab.boundingBoxSize}");
      Api.Log($"Rot: {prefab.rotation}");
      Api.Log($"Y Off: {prefab.yOffsetOfPrefab}");

      switch (prefab.CheckForAnyPlayerHome(GameManager.Instance.World))
      {
        case GameUtils.EPlayerHomeType.None:
          Api.Log($"Homes: None");
          break;
        case GameUtils.EPlayerHomeType.Landclaim:
          Api.Log($"Homes: Landclaim");
          break;
        case GameUtils.EPlayerHomeType.Bedroll:
          Api.Log($"Homes: Bedroll");
          break;
      }
    }
  }
}
