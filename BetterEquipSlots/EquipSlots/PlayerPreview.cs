using System;
using System.Reflection;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

internal static class PlayerPreview
{
	private static readonly Vector3 Origin = new Vector3(0f, 5000f, 0f);

	private const float FieldOfView = 26f;

	private const float ViewHeight = 2.7f;

	private const float DefaultYaw = -18f;

	private const float DragDegreesPerPixel = 0.5f;

	// Behind a BodyReplacer body, which leaves the texture's alpha as it finds it
	private static readonly Color SolidBackground = new Color(0.11f, 0.1f, 0.09f, 1f);

	private static GameObject s_holder;

	private static Transform s_model;

	private static Animator s_animator;

	private static float s_blockUntil;

	private static readonly int StateF = Animator.StringToHash("statef");

	private static readonly int StateI = Animator.StringToHash("statei");

	private static readonly int Blocking = Animator.StringToHash("blocking");

	private const float BlockSeconds = 0.9f;

	private static float s_yaw = DefaultYaw;

	// Zoom, 0 the whole body to 1 the head and shoulders, eased towards where the mouse wheel put it
	private static float s_zoom;

	private static float s_zoomTarget;

	private static float s_zoomSpeed;

	private const float ZoomPerNotch = 0.2f;

	private const float ZoomSeconds = 0.05f;

	// The camera's distance and the height it looks at, the whole body and zoomed in
	private const float NearDistance = 1.2f;

	private const float FarHeight = 0.92f;

	private const float NearHeight = 1.55f;

	// Up and down the body, by dragging (m), the height looked at kept between these
	private static float s_pan;

	private const float PanPerPixel = 0.004f;

	private const float LowestLook = 0.15f;

	private const float HighestLook = 1.85f;

	// Frames without a render after which the panel counts as closed, and the view goes back to the start when it opens
	private const int ClosedFrames = 2;

	private static VisEquipment s_vis;

	private static VisEquipment s_source;

	private static Camera s_camera;

	private static RenderTexture s_texture;

	private static Vector2 s_aspect = new Vector2(200f, 400f);

	private static int s_lastFrame = -10;

	private static bool s_failed;

	internal static void SetAspect(Vector2 size)
	{
		if (size.x > 1f && size.y > 1f)
		{
			s_aspect = size;
			ReleaseTexture();
		}
	}

	internal static Texture Render(Player player)
	{
		if (s_failed || player == null)
		{
			return null;
		}
		try
		{
			if (s_holder == null && !Create())
			{
				return null;
			}
			if (Time.frameCount - s_lastFrame > ClosedFrames)
			{
				ResetView();
			}
			EnsureTexture();
			s_source = ((Humanoid)player).m_visEquipment;
			((Humanoid)player).SetupVisEquipment(s_vis, false);
			CopyHoldPose(player);
			s_camera.backgroundColor = (BodyReplacerHost.Drawn(s_model.gameObject) ? SolidBackground : Color.clear);
			s_camera.enabled = true;
			s_lastFrame = Time.frameCount;
			return s_texture;
		}
		catch (Exception arg)
		{
			s_failed = true;
			Plugin.Log.LogError($"Character preview failed, switched off for this session: {arg}");
			Destroy();
			return null;
		}
	}

	// The player the preview copy shows, for mods that read a look from the VisEquipment's Player or ZDO
	internal static VisEquipment SourceOf(VisEquipment vis)
	{
		if (vis == null || vis != s_vis || s_source == null)
		{
			return null;
		}
		return s_source;
	}

	internal static void Tick()
	{
		if (s_zoom != s_zoomTarget)
		{
			s_zoom = Mathf.SmoothDamp(s_zoom, s_zoomTarget, ref s_zoomSpeed, ZoomSeconds, float.PositiveInfinity, Time.unscaledDeltaTime);
			if (Mathf.Abs(s_zoom - s_zoomTarget) < 0.001f)
			{
				s_zoom = s_zoomTarget;
				s_zoomSpeed = 0f;
			}
			PlaceCamera();
		}
		if (s_animator != null && s_blockUntil > 0f && Time.time >= s_blockUntil)
		{
			s_blockUntil = 0f;
			s_animator.SetBool(Blocking, value: false);
		}
		if (s_camera != null && s_camera.enabled && Time.frameCount - s_lastFrame > 1)
		{
			s_camera.enabled = false;
		}
	}

	private static bool Create()
	{
		GameObject gameObject = ((Game.instance != null) ? Game.instance.m_playerPrefab : null);
		if (gameObject == null)
		{
			return false;
		}
		ReleaseTexture();
		s_holder = new GameObject("CDEV_EquipmentSlots_Preview");
		s_holder.SetActive(value: false);
		s_holder.transform.position = Origin;
		bool forceDisableInit = ZNetView.m_forceDisableInit;
		ZNetView.m_forceDisableInit = true;
		try
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, Origin, Quaternion.Euler(0f, s_yaw, 0f), s_holder.transform);
			gameObject2.name = "Character";
			s_model = gameObject2.transform;
			Strip(gameObject2);
			s_vis = gameObject2.GetComponent<VisEquipment>();
			Animator[] componentsInChildren = gameObject2.GetComponentsInChildren<Animator>(includeInactive: true);
			foreach (Animator animator in componentsInChildren)
			{
				animator.updateMode = AnimatorUpdateMode.Normal;
				animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
				animator.fireEvents = false;
				if (s_animator == null && animator.runtimeAnimatorController != null)
				{
					s_animator = animator;
				}
			}
			GameObject gameObject3 = new GameObject("Camera");
			gameObject3.transform.SetParent(s_holder.transform, worldPositionStays: false);
			s_camera = gameObject3.AddComponent<Camera>();
			s_camera.clearFlags = CameraClearFlags.Color;
			s_camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			s_camera.fieldOfView = 26f;
			s_camera.nearClipPlane = 0.3f;
			s_camera.farClipPlane = 12f;
			s_camera.cullingMask = -33;
			s_camera.depth = -50f;
			s_camera.allowHDR = false;
			s_camera.allowMSAA = false;
			s_camera.enabled = false;
			PlaceCamera();
			GameObject gameObject4 = new GameObject("Light");
			gameObject4.transform.SetParent(s_holder.transform, worldPositionStays: false);
			gameObject4.transform.position = Origin + new Vector3(1.2f, 2.2f, 2.5f);
			Light light = gameObject4.AddComponent<Light>();
			light.type = LightType.Point;
			light.range = 10f;
			light.intensity = 1.3f;
			light.color = new Color(1f, 0.93f, 0.82f);
			light.shadows = LightShadows.None;
			s_holder.SetActive(value: true);
			SkipIntro(gameObject2);
		}
		finally
		{
			ZNetView.m_forceDisableInit = forceDisableInit;
		}
		if (s_vis == null)
		{
			Plugin.Log.LogWarning("The player prefab has no VisEquipment; no character preview.");
			s_failed = true;
			Destroy();
			return false;
		}
		return true;
	}

	private static void SkipIntro(GameObject model)
	{
		Animator[] componentsInChildren = model.GetComponentsInChildren<Animator>(includeInactive: true);
		foreach (Animator animator in componentsInChildren)
		{
			if (animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
			{
				for (int j = 0; j < 40; j++)
				{
					animator.Update(0.25f);
				}
			}
		}
	}

	private static void CopyHoldPose(Player player)
	{
		Animator animator = ((Character)player).m_animator;
		if (!(s_animator == null) && !(animator == null))
		{
			float num = animator.GetFloat(StateF);
			int integer = animator.GetInteger(StateI);
			if (!Mathf.Approximately(s_animator.GetFloat(StateF), num))
			{
				s_animator.SetFloat(StateF, num);
			}
			if (s_animator.GetInteger(StateI) != integer)
			{
				s_animator.SetInteger(StateI, integer);
			}
		}
	}

	internal static void Attack()
	{
		Player localPlayer = Player.m_localPlayer;
		if (s_animator == null || localPlayer == null)
		{
			return;
		}
		Attack attack = localPlayer.GetCurrentWeapon()?.m_shared?.m_attack;
		if (attack != null && !string.IsNullOrEmpty(attack.m_attackAnimation))
		{
			s_blockUntil = 0f;
			s_animator.SetBool(Blocking, value: false);
			string text = attack.m_attackAnimation;
			if (attack.m_attackChainLevels > 1 || attack.m_attackRandomAnimations >= 2)
			{
				text += "0";
			}
			s_animator.SetTrigger(text);
		}
	}

	internal static void Block()
	{
		if (!(s_animator == null))
		{
			s_animator.SetBool(Blocking, value: true);
			s_blockUntil = Time.time + 0.9f;
		}
	}

	// The whole body fits the height of the view at this distance
	private static float FarDistance => 1.35f / Mathf.Tan(FieldOfView * 0.5f * Mathf.Deg2Rad);

	private static void PlaceCamera()
	{
		if (s_camera == null)
		{
			return;
		}
		// Each notch brings the camera the same share closer, so every one of them is seen
		float t = s_zoom;
		float distance = FarDistance * Mathf.Pow(NearDistance / FarDistance, t);
		float height = Mathf.Lerp(FarHeight, NearHeight, Mathf.InverseLerp(FarDistance, NearDistance, distance));
		s_pan = Mathf.Clamp(s_pan, LowestLook - height, HighestLook - height);
		height += s_pan;
		Transform transform = s_camera.transform;
		transform.position = Origin + new Vector3(0f, height, distance);
		transform.LookAt(Origin + new Vector3(0f, height, 0f));
	}

	internal static void Zoom(float notches)
	{
		s_zoomTarget = Mathf.Clamp01(s_zoomTarget + Mathf.Clamp(notches, -1f, 1f) * ZoomPerNotch);
	}

	internal static void Pan(float pixels)
	{
		s_pan -= pixels * PanPerPixel;
		PlaceCamera();
	}

	private static void ResetView()
	{
		s_zoom = s_zoomTarget = s_zoomSpeed = s_pan = 0f;
		s_yaw = DefaultYaw;
		if (s_model != null)
		{
			s_model.rotation = Quaternion.Euler(0f, s_yaw, 0f);
		}
		PlaceCamera();
	}

	internal static void Turn(float pixels)
	{
		s_yaw = Mathf.Repeat(s_yaw - pixels * 0.5f + 180f, 360f) - 180f;
		if (s_model != null)
		{
			s_model.rotation = Quaternion.Euler(0f, s_yaw, 0f);
		}
	}

	private static void Strip(GameObject model)
	{
		Assembly assembly = typeof(Player).Assembly;
		Character[] componentsInChildren = model.GetComponentsInChildren<Character>(includeInactive: true);
		foreach (Character obj in componentsInChildren)
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
		for (int j = 0; j < 2; j++)
		{
			MonoBehaviour[] componentsInChildren2 = model.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
			foreach (MonoBehaviour monoBehaviour in componentsInChildren2)
			{
				if (monoBehaviour != null && monoBehaviour.GetType().Assembly == assembly && !(monoBehaviour is VisEquipment) && !(monoBehaviour is ZNetView))
				{
					UnityEngine.Object.DestroyImmediate(monoBehaviour);
				}
			}
		}
		Collider[] componentsInChildren3 = model.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider obj2 in componentsInChildren3)
		{
			UnityEngine.Object.DestroyImmediate(obj2);
		}
		Rigidbody[] componentsInChildren4 = model.GetComponentsInChildren<Rigidbody>(includeInactive: true);
		foreach (Rigidbody obj3 in componentsInChildren4)
		{
			UnityEngine.Object.DestroyImmediate(obj3);
		}
	}

	private static void EnsureTexture()
	{
		int num = Mathf.Clamp(Mathf.RoundToInt(s_aspect.x * 2f), 64, 1024);
		int num2 = Mathf.Clamp(Mathf.RoundToInt(s_aspect.y * 2f), 64, 2048);
		if (!(s_texture != null) || s_texture.width != num || s_texture.height != num2)
		{
			ReleaseTexture();
			s_texture = new RenderTexture(num, num2, 24, RenderTextureFormat.ARGB32)
			{
				name = "CDEV_EquipmentSlots_Preview"
			};
			s_texture.Create();
			s_camera.targetTexture = s_texture;
			s_camera.aspect = (float)num / (float)num2;
		}
	}

	private static void ReleaseTexture()
	{
		if (s_camera != null)
		{
			s_camera.targetTexture = null;
		}
		if (s_texture != null)
		{
			s_texture.Release();
			UnityEngine.Object.Destroy(s_texture);
			s_texture = null;
		}
	}

	internal static void Destroy()
	{
		ReleaseTexture();
		if (s_holder != null)
		{
			UnityEngine.Object.Destroy(s_holder);
		}
		s_holder = null;
		s_model = null;
		s_animator = null;
		s_vis = null;
		s_source = null;
		s_camera = null;
	}
}
