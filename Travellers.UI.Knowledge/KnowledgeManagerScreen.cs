using System.Collections;
using System.Collections.Generic;
using Bossa.Travellers.Scanning;
using BossalyticsNS;
using Improbable.Collections;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using Travellers.UI.PlayerInventory;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.Knowledge;

public class KnowledgeManagerScreen : UIScreenComponent
{
	public enum KnowledgeAnimationType
	{
		CURRENT_NODE_HIGHLIGHT = 1,
		SPAWN_SCHEMATIC,
		BRANCH_HIGHLIGHT,
		PANEL_LOWLIGHT,
		TARGET_BRANCH,
		NONE
	}

	private const int _MAX_KNOWLEDGE_POINTS = 10000;

	public Image knowledgeBar;

	public Text knowledgeValue;

	public ScrollRect scrollRect;

	public GameObject[] lockedKnowledgeObjects;

	public GameObject[] unlockedKnowledgeObjects;

	public KnowledgeBranch[] allBranches;

	private Dictionary<string, KnowledgeBranch> branchByCategory = new Dictionary<string, KnowledgeBranch>();

	public System.Collections.Generic.List<KnowledgeNode> rootNodes;

	public KnowledgeNode[] allNodes;

	public System.Collections.Generic.List<KnowledgeConnection> allConnections = new System.Collections.Generic.List<KnowledgeConnection>();

	public Dictionary<string, KnowledgeNode> nodesById = new Dictionary<string, KnowledgeNode>();

	private bool checkNeuronStates;

	public Transform treeRoot;

	public KnowledgeConnection connectionPrefab;

	public Transform categoryRoot;

	public KnowledgeNode currentNode;

	public RectTransform scrollerTr;

	public float parallaxFactor = 0.8f;

	public RectTransform[] parallaxLayers;

	public ScrollRectZoom scrollRectZoom;

	public float NeuronMessageSpeed = 1f;

	public int animationFlag;

	public Animator nodePurchasedAnimator;

	public Animator sparksAnimator;

	private RectTransform nodePurchasedRectTr;

	private AtomicCoroutineRunner currentAnimation = new AtomicCoroutineRunner();

	private AtomicCoroutineRunner spawningAnimation = new AtomicCoroutineRunner();

	private bool _isAutoDragging;

	private Vector2 _dragDir;

	private bool isUnlocked;

	public GameObject leftScroll;

	public GameObject rightScroll;

	public GameObject topScroll;

	public GameObject bottomScroll;

	public RectTransform scrollBar;

	public RectTransform scrollArrow;

	public static KnowledgeManagerScreen Instance;

	private const string ShipbuildingNodeId = "Shipbuilding";

	private int knowledgePoints = 1000;

	private InventorySystem _inventorysystem;

	public float targetBranchSpeed = 1f;

	public float minTargetBranchTime = 0.25f;

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorysystem)
	{
		_inventorysystem = inventorysystem;
	}

	protected override void ProtectedInit()
	{
		Instance = this;
		Connect();
		HideAll();
		if (GlobalKnowledgeGraphDataVisualizer.IsCheckedOut)
		{
			OnKnowledgeGraphUpdate(GlobalKnowledgeGraphDataVisualizer.GetKnowledgeGraph());
		}
		OnKnowledgeNodeUsesUpdated(LocalPlayer.Instance.scanningAgentVisualizer.GetKnowledgeNodesUses());
		UpdateTutorialStartPopups();
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIPlayerProfileEvents.UpdateKnowledge, OnKnowledgeUpdated);
	}

	protected override void Activate()
	{
		UpdateTutorialSteps();
		scrollerTr.anchoredPosition = Vector2.zero;
		OnScrollerDragged(Vector2.zero);
		scrollRect.enabled = true;
		if (nodePurchasedRectTr == null && nodePurchasedAnimator != null)
		{
			nodePurchasedRectTr = nodePurchasedAnimator.gameObject.GetComponent<RectTransform>();
		}
		if (scrollRectZoom == null && scrollerTr != null)
		{
			scrollRectZoom = scrollerTr.GetComponent<ScrollRectZoom>();
		}
		scrollRectZoom.OnScrollValueChanged += OnScrollRectZoomChanged;
		if (LocalPlayer.Exists && LocalPlayer.Instance.scanningAgentVisualizer != null)
		{
			GlobalKnowledgeGraphDataVisualizer.KnowledgeGraphUpdated += OnKnowledgeGraphUpdate;
			LocalPlayer.Instance.scanningAgentVisualizer.KnowledgeUseResponse += OnKnowledgeUseResponse;
			LocalPlayer.Instance.scanningAgentVisualizer.KnowledgeNodeUsesUpdated += OnKnowledgeNodeUsesUpdated;
		}
		InitNeurons();
		CheckAllNeuronsState();
		DisplayKnowledgePoints();
		ChangeNeuronPositions();
	}

	protected override void Deactivate()
	{
		if (LocalPlayer.Exists && LocalPlayer.Instance.scanningAgentVisualizer != null)
		{
			GlobalKnowledgeGraphDataVisualizer.KnowledgeGraphUpdated -= OnKnowledgeGraphUpdate;
			LocalPlayer.Instance.scanningAgentVisualizer.KnowledgeUseResponse -= OnKnowledgeUseResponse;
			LocalPlayer.Instance.scanningAgentVisualizer.KnowledgeNodeUsesUpdated -= OnKnowledgeNodeUsesUpdated;
		}
		StopDrag();
		currentAnimation.Stop();
		if (animationFlag > 0)
		{
			for (int i = 1; i < 6; i++)
			{
				KnowledgeAnimationType type = (KnowledgeAnimationType)i;
				if (IsAnimating(type))
				{
					StopAnimating(type);
				}
			}
		}
		spawningAnimation.Stop();
		if (sparksAnimator != null)
		{
			sparksAnimator.StopPlayback();
			sparksAnimator.Rebind();
			sparksAnimator.gameObject.SetActive(value: false);
		}
		currentNode = null;
		scrollRect.enabled = true;
		scrollRectZoom.OnScrollValueChanged -= OnScrollRectZoomChanged;
		KnowledgeNode.isPurchasing = false;
	}

	protected void Update()
	{
		if (_isAutoDragging)
		{
			if (_dragDir.magnitude == 0f)
			{
				StopDrag();
			}
			else
			{
				scrollRect.verticalNormalizedPosition += _dragDir.y * Time.deltaTime;
				scrollRect.horizontalNormalizedPosition += _dragDir.x * Time.deltaTime;
			}
		}
		if (_inventorysystem.UpdateKnowledgeSchematics)
		{
			InitNeurons();
		}
	}

	private void InitNeurons()
	{
		if (GlobalKnowledgeGraphDataVisualizer.IsCheckedOut)
		{
			OnKnowledgeGraphUpdate(GlobalKnowledgeGraphDataVisualizer.GetKnowledgeGraph());
		}
		if (LocalPlayer.Exists && LocalPlayer.Instance.scanningAgentVisualizer != null)
		{
			OnKnowledgeUpdated(LocalPlayer.Instance.scanningAgentVisualizer.GetKnowledgePoints());
			OnKnowledgeNodeUsesUpdated(LocalPlayer.Instance.scanningAgentVisualizer.GetKnowledgeNodesUses());
		}
		bool flag = (isUnlocked = true);
		if (unlockedKnowledgeObjects != null)
		{
			for (int i = 0; i < unlockedKnowledgeObjects.Length; i++)
			{
				if (unlockedKnowledgeObjects[i] != null)
				{
					unlockedKnowledgeObjects[i].SetActive(flag);
				}
			}
		}
		if (lockedKnowledgeObjects != null)
		{
			for (int j = 0; j < lockedKnowledgeObjects.Length; j++)
			{
				if (lockedKnowledgeObjects[j] != null)
				{
					lockedKnowledgeObjects[j].SetActive(!flag);
				}
			}
		}
		topScroll.SetActive(isUnlocked);
		bottomScroll.SetActive(isUnlocked);
		leftScroll.SetActive(isUnlocked);
		rightScroll.SetActive(isUnlocked);
		scrollBar.transform.parent.gameObject.SetActive(isUnlocked);
		scrollRectZoom.SetScrollValue(scrollRectZoom.currentScrollValue);
		OnScrollerDragged(scrollRect.normalizedPosition);
		CheckAllNeuronsState();
	}

	private void LateUpdate()
	{
		if (checkNeuronStates)
		{
			doCheckAllNeuronsState();
		}
	}

	private void DisplayKnowledgePoints()
	{
		int num = GetKnowledgePoints();
		if (knowledgeBar != null)
		{
			knowledgeBar.fillAmount = (float)num / 10000f;
		}
		if (knowledgeValue != null)
		{
			knowledgeValue.text = $"{num}";
		}
	}

	private void HideAll()
	{
		for (int i = 0; i < allNodes.Length; i++)
		{
			if (allNodes[i] != null && !rootNodes.Contains(allNodes[i]))
			{
				allNodes[i].Hide();
			}
		}
	}

	private void ChangeNeuronPositions()
	{
	}

	public void CheckAllNeuronsState(bool force = false)
	{
		if (force)
		{
			doCheckAllNeuronsState();
		}
		else
		{
			checkNeuronStates = true;
		}
	}

	private void doCheckAllNeuronsState()
	{
		for (int i = 0; i < allBranches.Length; i++)
		{
			allBranches[i].CheckStates();
		}
		for (int j = 0; j < allConnections.Count; j++)
		{
			if (allConnections[j] != null)
			{
				allConnections[j].Refresh();
			}
		}
		checkNeuronStates = false;
	}

	public bool IsAnimating()
	{
		return animationFlag != 0;
	}

	public bool IsAnimating(KnowledgeAnimationType type)
	{
		return (animationFlag & (1 << (int)type)) != 0;
	}

	public void StartAnimating(KnowledgeAnimationType type)
	{
		animationFlag |= 1 << (int)type;
	}

	public void StopAnimating(KnowledgeAnimationType type)
	{
		animationFlag &= ~(1 << (int)type);
	}

	[ContextMenu("Connect")]
	public void Connect()
	{
		allBranches = GetComponentsInChildren<KnowledgeBranch>(includeInactive: false);
		allNodes = GetComponentsInChildren<KnowledgeNode>(includeInactive: false);
		rootNodes.Clear();
		branchByCategory.Clear();
		KnowledgeCategory[] componentsInChildren = categoryRoot.GetComponentsInChildren<KnowledgeCategory>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != null)
			{
				Object.DestroyImmediate(componentsInChildren[i].gameObject);
			}
		}
		for (int j = 0; j < allBranches.Length; j++)
		{
			allBranches[j].Connect();
			rootNodes.Add(allBranches[j].rootNode);
			branchByCategory.Add(allBranches[j].category, allBranches[j]);
		}
		categoryRoot.gameObject.SetActive(value: false);
		allConnections.Clear();
		KnowledgeConnection[] componentsInChildren2 = GetComponentsInChildren<KnowledgeConnection>(includeInactive: true);
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			if (componentsInChildren2[k] != null)
			{
				if (!componentsInChildren2[k].isNonHierarchical)
				{
					Object.DestroyImmediate(componentsInChildren2[k].gameObject);
					continue;
				}
				allConnections.Add(componentsInChildren2[k]);
				componentsInChildren2[k].Connect();
			}
		}
		for (int l = 0; l < allNodes.Length; l++)
		{
			allNodes[l].outGoingConnections.Clear();
			allNodes[l].Resize();
			if (string.IsNullOrEmpty(allNodes[l].nodeData.id))
			{
				Debug.LogError("KnowledgeNode id empty on " + allNodes[l].gameObject.name);
			}
		}
		for (int m = 0; m < allNodes.Length; m++)
		{
			if (!(allNodes[m] != null))
			{
				continue;
			}
			allNodes[m].originalPosition = allNodes[m].rectTr.anchoredPosition;
			for (int n = 0; n < allNodes[m].parentNodes.Length; n++)
			{
				GameObject gameObject = UIObjectFactory.Instantiate(connectionPrefab.gameObject, Vector3.zero, Quaternion.identity);
				if (gameObject != null)
				{
					gameObject.transform.SetParent(treeRoot);
					gameObject.transform.SetSiblingIndex(0);
					KnowledgeConnection component = gameObject.GetComponent<KnowledgeConnection>();
					if (component != null)
					{
						allConnections.Add(component);
						component.fromNode = allNodes[m].parentNodes[n];
						component.toNode = allNodes[m];
						component.Connect();
					}
				}
			}
			for (int num = 0; num < rootNodes.Count; num++)
			{
				if (!(allNodes[m].rootNode == null))
				{
					break;
				}
				if (rootNodes[num].transform.parent.parent == allNodes[m].transform.parent.parent)
				{
					allNodes[m].rootNode = rootNodes[num];
				}
			}
		}
		nodesById.Clear();
		for (int num2 = 0; num2 < allNodes.Length; num2++)
		{
			allNodes[num2].Connect();
			nodesById.Add(allNodes[num2].nodeData.id, allNodes[num2]);
		}
	}

	public void TargetBranch(KnowledgeBranch branch)
	{
		if (!(branch == null))
		{
			currentAnimation.Start(iTargetBranch(branch));
		}
	}

	private IEnumerator iTargetBranch(KnowledgeBranch branch)
	{
		StartAnimating(KnowledgeAnimationType.TARGET_BRANCH);
		float fromScale = scrollRectZoom.currentScrollValue;
		float toScale = 1f;
		Vector2 fromPos = scrollRect.normalizedPosition;
		Vector2 toPos = branch.scrollerPosition;
		float dist = (toPos - fromPos).magnitude;
		float startTime = Time.time;
		float time = dist / targetBranchSpeed;
		if (time < minTargetBranchTime)
		{
			time = minTargetBranchTime;
		}
		while (Time.time - startTime <= time)
		{
			float pct = (Time.time - startTime) / time;
			if (pct < 0f)
			{
				pct = 0f;
			}
			if (pct >= 1f)
			{
				pct = 1f;
			}
			scrollRectZoom.SetScrollValue(fromScale + (toScale - fromScale) * pct);
			scrollRect.normalizedPosition = fromPos + (toPos - fromPos) * pct;
			yield return null;
		}
		scrollRectZoom.SetScrollValue(toScale);
		scrollRect.normalizedPosition = toPos;
		StopAnimating(KnowledgeAnimationType.TARGET_BRANCH);
	}

	public void OnScrollerDragged(Vector2 pos)
	{
		if (!isUnlocked)
		{
			return;
		}
		leftScroll.SetActive(isUnlocked && scrollRect.normalizedPosition.x > 0f);
		rightScroll.SetActive(isUnlocked && scrollRect.normalizedPosition.x < 1f);
		bottomScroll.SetActive(isUnlocked && scrollRect.normalizedPosition.y > 0f);
		topScroll.SetActive(isUnlocked && scrollRect.normalizedPosition.y < 1f);
		if (_isAutoDragging)
		{
			if (!leftScroll.activeSelf && _dragDir.x < 0f)
			{
				StopDrag();
			}
			else if (!rightScroll.activeSelf && _dragDir.x > 0f)
			{
				StopDrag();
			}
			else if (!bottomScroll.activeSelf && _dragDir.y < 0f)
			{
				StopDrag();
			}
			else if (!topScroll.activeSelf && _dragDir.y > 0f)
			{
				StopDrag();
			}
		}
		float num = parallaxFactor;
		Vector2 vector = scrollerTr.localPosition;
		for (int i = 0; i < parallaxLayers.Length; i++)
		{
			parallaxLayers[i].localPosition = vector * num;
			num *= parallaxFactor;
		}
	}

	[ContextMenu("DragLeft")]
	public void DragLeft()
	{
		Drag(Vector2.left);
	}

	[ContextMenu("DragRight")]
	public void DragRight()
	{
		Drag(Vector2.right);
	}

	[ContextMenu("DragUp")]
	public void DragUp()
	{
		Drag(Vector2.up);
	}

	[ContextMenu("DragDown")]
	public void DragDown()
	{
		Drag(Vector2.down);
	}

	public void Drag(Vector2 dir)
	{
		if (!_isAutoDragging)
		{
			_isAutoDragging = true;
			_dragDir = dir;
		}
	}

	public void StopDrag()
	{
		_isAutoDragging = false;
		_dragDir = Vector2.zero;
	}

	public void OnScrollRectZoomChanged(float value)
	{
		if (isUnlocked)
		{
			Vector2 anchoredPosition = scrollArrow.anchoredPosition;
			anchoredPosition.y = value * scrollBar.sizeDelta.y;
			scrollArrow.anchoredPosition = anchoredPosition;
		}
	}

	private void OnKnowledgeUpdated(object[] objects)
	{
		OnKnowledgeUpdated((int)objects[0]);
	}

	private void OnKnowledgeUpdated(int points)
	{
		knowledgePoints = points;
		DisplayKnowledgePoints();
	}

	public int GetKnowledgePoints()
	{
		if (LocalPlayer.Exists && LocalPlayer.Instance.scanningAgentVisualizer != null)
		{
			return LocalPlayer.Instance.scanningAgentVisualizer.GetKnowledgePoints();
		}
		return knowledgePoints;
	}

	public void PurchaseKnowledge(KnowledgeNode node)
	{
		if (LocalPlayer.Exists && LocalPlayer.Instance.scanningAgentVisualizer != null)
		{
			currentNode = node;
			LocalPlayer.Instance.scanningAgentVisualizer.UseKnowledgeNode(node.nodeData.id);
		}
	}

	private void OnKnowledgeGraphUpdate(string json)
	{
		bool flag = false;
		IDictionary dictionary = (IDictionary)MiniJSON.jsonDecode(json);
		if (dictionary.Contains("unlockedByDefault"))
		{
			IDictionary dictionary2 = (IDictionary)dictionary["unlockedByDefault"];
			foreach (string key in dictionary2.Keys)
			{
				IDictionary dicNode = (IDictionary)dictionary2[key];
				if (nodesById.ContainsKey(key) && nodesById[key] != null)
				{
					bool flag2 = nodesById[key].FromIDictionary(dicNode);
					flag = flag || flag2;
				}
				else
				{
					Hack_LogErrorIfNotCipher(key);
				}
			}
		}
		if (dictionary.Contains("unlockedByOtherNodes"))
		{
			IDictionary dictionary3 = (IDictionary)dictionary["unlockedByOtherNodes"];
			foreach (string key2 in dictionary3.Keys)
			{
				IDictionary dicNode2 = (IDictionary)dictionary3[key2];
				if (nodesById.ContainsKey(key2) && nodesById[key2] != null)
				{
					bool flag3 = nodesById[key2].FromIDictionary(dicNode2);
					flag = flag || flag3;
				}
				else
				{
					Hack_LogErrorIfNotCipher(key2);
				}
			}
		}
		if (dictionary.Contains("fixedSchematicsNodes"))
		{
			IDictionary dictionary4 = (IDictionary)dictionary["fixedSchematicsNodes"];
			foreach (string key3 in dictionary4.Keys)
			{
				IDictionary dicNode3 = (IDictionary)dictionary4[key3];
				if (nodesById.ContainsKey(key3) && nodesById[key3] != null)
				{
					bool flag4 = nodesById[key3].DetailsFromIDictionary(dicNode3);
					flag = flag || flag4;
				}
				else
				{
					Hack_LogErrorIfNotCipher(key3);
				}
			}
		}
		if (!dictionary.Contains("proceduralSchematicsNodes"))
		{
			return;
		}
		IDictionary dictionary5 = (IDictionary)dictionary["proceduralSchematicsNodes"];
		foreach (string key4 in dictionary5.Keys)
		{
			IDictionary dicNode4 = (IDictionary)dictionary5[key4];
			if (nodesById.ContainsKey(key4) && nodesById[key4] != null)
			{
				bool flag5 = nodesById[key4].DetailsFromIDictionary(dicNode4);
				flag = flag || flag5;
			}
			else
			{
				Hack_LogErrorIfNotCipher(key4);
			}
		}
	}

	private void OnKnowledgeNodeUsesUpdated(Map<string, int> nodes)
	{
		bool flag = false;
		KnowledgeNode node = null;
		bool flag2 = false;
		foreach (KeyValuePair<string, int> node2 in nodes)
		{
			if (!nodesById.ContainsKey(node2.Key))
			{
				continue;
			}
			KnowledgeNode knowledgeNode = nodesById[node2.Key];
			if (knowledgeNode != null && knowledgeNode.nodeData != null)
			{
				if (knowledgeNode.nodeData.currentUses != node2.Value)
				{
					knowledgeNode.nodeData.currentUses = node2.Value;
					flag = true;
					node = knowledgeNode;
				}
				if (knowledgeNode.nodeData.nodeType != 0 && node2.Value >= 1)
				{
					knowledgeNode.nodeData.purchased = true;
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIPlayerProfileEvents.KnowledgeNodesUpdated, new KnowledgeNodesUpdated());
		}
		if (flag)
		{
			currentAnimation.Stop();
			if (IsAnimating(KnowledgeAnimationType.BRANCH_HIGHLIGHT))
			{
				StopAnimating(KnowledgeAnimationType.BRANCH_HIGHLIGHT);
				CheckAllNeuronsState();
			}
			currentAnimation.Start(iPlayNodePurchaseAnimation(node));
			UpdateTutorialSteps();
			UpdateTutorialStartPopups();
		}
	}

	private IEnumerator iPlayNodePurchaseAnimation(KnowledgeNode node)
	{
		scrollRect.enabled = false;
		if (currentNode != null && currentNode.branch != null)
		{
			yield return StartCoroutine(currentNode.branch.iUnlockNode(currentNode));
		}
		scrollRect.enabled = true;
		currentNode = null;
		CheckAllNeuronsState();
	}

	private void OnKnowledgeUseResponse(KnowledgeUseResponse e)
	{
		switch (e.response)
		{
		case KnowledgeUseResponseType.InexistentNode:
			DialogPopupFacade.ShowOkDialog("Inexistent node", "Node doesn't exist...");
			break;
		case KnowledgeUseResponseType.NotEnoughKnowledge:
			DialogPopupFacade.ShowOkDialog("Not enough Knowledge", "You don't have enough knowledge!");
			break;
		case KnowledgeUseResponseType.FullInventory:
			DialogPopupFacade.ShowOkDialog("Inventory full", "Make some room in your inventory!");
			break;
		case KnowledgeUseResponseType.NodeLocked:
			DialogPopupFacade.ShowOkDialog("Node locked", "You must unlock this node before you can purchase it!");
			break;
		}
		KnowledgeNode.isPurchasing = false;
	}

	protected override void ProtectedDispose()
	{
	}

	private void UpdateTutorialSteps()
	{
		if (IsShipbuildingAware())
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(TutorialStep.LEARN_SHIPBUILDING));
		}
		else
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.LEARN_SHIPBUILDING));
		}
	}

	private void UpdateTutorialStartPopups()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIPlayerProfileEvents.UpdateShipbuildingAware, IsShipbuildingAware());
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIPlayerProfileEvents.UpdateKnowledge, GetKnowledgePoints());
	}

	private bool IsShipbuildingAware()
	{
		return !nodesById.ContainsKey("Shipbuilding") || nodesById["Shipbuilding"].nodeData == null || nodesById["Shipbuilding"].nodeData.isPurchased;
	}

	private void Hack_LogErrorIfNotCipher(string slotKey)
	{
		if (!slotKey.ToLower().Contains("cipher"))
		{
			WALogger.Error<KnowledgeManagerScreen>("Error: no node for {0}", new object[1] { slotKey });
		}
	}
}
