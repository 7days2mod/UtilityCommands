using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StompyNZ.Commands
{
  public class PrefabUtils : IConsoleCommand
  {
    public PrefabUtils()
    {
    }

    public string[] GetCommands() =>
      new[] { "prefabutil", "pre" };

    public string GetDescription() =>
      "A command for displaying prefab instance info and resetting prefabs";

    public string GetHelp() =>
      "prefabutil list [range] - list name and location of all dynamic prefabs, or those within range\n" +
      "prefabutil show [id] - list details about the nearest prefab or the prefab with id given\n" +
      "prefabutil reset [id] - reset the nearest prefab or the prefab with given id\n";

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

      if (_params.Count == 0)
      {
        Api.Log("A sub command is required.");

        return;
      }

      var subCommand = _params[0].ToLower();

      Process(subCommand, _params, player);
    }

    private static void Process(string subCommand, IList<string> _params, EntityPlayer player)
    {
      var tag = QuestTags.none;

      var decorator = GameManager.Instance.GetDynamicPrefabDecorator();

      switch (subCommand)
      {
        case "list":
          {
            List<PrefabInstance> prefabs;
            if (_params.Count > 1)
            {
              if (!float.TryParse(_params[1], out var range))
              {
                Api.Log($"Unable to parse range {_params[1]} as a number.");

                return;
              }

              var prefabsDict = new Dictionary<int, PrefabInstance>();
              decorator.GetPrefabsAround(_middlePos: player.position, _nearDistance: 0, _farDistance: range, _prefabsFar: prefabsDict, _prefabsNear: null, _bAllPrefabs: true);
              prefabs = prefabsDict.Values.ToList();
            }
            else
            {
              prefabs = decorator.GetDynamicPrefabs();
            }

            foreach (var p in prefabs)
            {
              Api.Log($"{p.name} @ {p.boundingBoxPosition.x},{p.boundingBoxPosition.y},{p.boundingBoxPosition.z}");
            }

            return;
          }

        case "reset":
          {
            var prefab = GetPrefab(_params, player.position, tag, decorator);
            if (prefab == null) { return; }

            if (prefab.CheckForAnyPlayerHome(GameManager.Instance.World) != GameUtils.EPlayerHomeType.None)
            {
              Api.Log("Can't reset a prefab that has been claimed in the past.");

              return;
            }

            Api.Log($"Resetting prefab @{prefab.boundingBoxPosition}");

            prefab.Reset(GameManager.Instance.World, tag);

            return;
          }

        case "show":
          {
            var prefab = GetPrefab(_params, player.position, tag, decorator);
            if (prefab == null) { return; }

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

            return;
          }

        default:
          Api.Log($"Unknown sub command {subCommand}");
          return;
      }
    }

    private static PrefabInstance GetPrefab(IList<string> _params, Vector3 position, QuestTags tag, DynamicPrefabDecorator decorator)
    {
      PrefabInstance prefab;
      if (_params.Count > 1)
      {
        if (!int.TryParse(_params[1], out var id))
        {
          Api.Log($"Unable to parse {_params[1]} as a number");

          return null;
        }
        prefab = decorator.GetPrefab(id);
      }
      else
      {
        prefab = decorator.GetClosestPOIToWorldPos(tag, position);
      }

      return prefab;
    }
  }
}
