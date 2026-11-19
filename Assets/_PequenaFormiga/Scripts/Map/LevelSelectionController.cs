using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelSelectionController : MonoBehaviour
{
  [SerializeField] private MapPlayer player;
  [SerializeField] private LevelTooltipController tooltip;
  [Tooltip("O primeiro elemento é o nó inicial")]
  [SerializeField] private MapNode[] allNodes;

  private MapNode currNode;
  private bool isLoading = false;

  void Awake()
  {
    bool isValid = true;

    if (!player)
    {
      Debug.LogError("Player not assigned", this);
      isValid = false;
    }
    if (!tooltip)
    {
      Debug.LogError("Tooltip Script not assigned", this);
      isValid = false;
    }
    if (!ValidateNodeArray())
    {
      isValid = false;
    }

    if (!isValid)
    {
      enabled = false;
      return;
    }
  }

  void OnEnable()
  {
    if (!player)
      return;
    player.OnConfirmInput += HandleInteract;
    player.OnMoveInput += HandleMovement;
  }

  void OnDisable()
  {
    if (!player)
      return;
    player.OnConfirmInput -= HandleInteract;
    player.OnMoveInput -= HandleMovement;
  }

  void Start()
  {
    int currLevel = GameSession.LastLevel;

    MapNode startNode = System.Array.Find(allNodes, level => level.HasLevel && level.Data.LevelNumber == currLevel);

    if (!startNode)
    {
      if (currLevel >= 0)
        Debug.LogWarning($"Level {currLevel} not found on map, using first node", this);
      startNode = allNodes[0];
    }

    SelectLevel(startNode);
  }

  private void SelectLevel(MapNode node)
  {
    //TODO: Transformar essa função em uma parte do HandleMovement, que sera a parte de ligar o tooltip e nivel
    player.SetPosition(node.StopPosition);

    if (!node.HasLevel)
    {
      currNode = null;
      tooltip.Hide();
      return;
    }
    currNode = node;
    tooltip.transform.position = node.TooltipAnchor.position;
    tooltip.Show(node.Data);

  }

  private void HandleInteract()
  {
    if (isLoading)
      return;
    if (!currNode)
      return;
    
    isLoading = true;
    SceneManager.LoadScene(currNode.Data.SceneName);
  } 

  private void HandleMovement(MapDirection direction)
  {
    if (isLoading)
      return;
    //TODO: Verificar se caminho é valido (outra função) e caso sim ligar função para MapPlayer
  } 

  private bool ValidateNodeArray()
  {
    if (!(allNodes.Length > 0))
    {
      Debug.LogError($"Node Array Empty", this);
      return false;
    }

    bool isValid = true;

    HashSet<int> levelNumbers = new HashSet<int>();

    for (int i = 0; i < allNodes.Length; i++)
    {
      var mapNode = allNodes[i];

      if (!mapNode)
      {
        Debug.LogError($"Null in {i+1}° array position", this);
        isValid = false;
        continue;
      }

      if (!mapNode.IsValid)
      {
        Debug.LogError($"Map Node isn't configured correctly at {i+1}° array position\nNode Name: {mapNode.name}", this);
        isValid = false;
        continue;
      }
      
      if (!mapNode.HasLevel)
        continue;
      if (!Application.CanStreamedLevelBeLoaded(mapNode.Data.SceneName))
      {
        Debug.LogError($"Scene Name invalid in {i+1}° array position.\nNode Name: {mapNode.name}\n-Scene Name: {mapNode.Data.SceneName}", this);
        isValid = false;
      }
      if (!levelNumbers.Add(mapNode.Data.LevelNumber))
      {
        Debug.LogError($"Level Number duplicated in {i+1}° array position.\nNode Name: {mapNode.name}\n- Level Number: {mapNode.Data.LevelNumber}", this);
        isValid = false;
      }
    }

    return isValid;
  }
}
