using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using HKTool;

namespace HKDebug.HitBox
{
    public class HitBoxCore
    {
		public static float lastUpdate = 0;
		public static HitBoxConfig HitBoxConfig => HKToolMod.settings.DebugConfig.HitBoxConfig;
		public static bool enableHitBox = false;
		public static void Init()
        {
			Menu.MenuManager.AddButton(new Menu.ButtonInfo()
			{
				label = "HKTool.Debug.EnableHitBox".Get(),
				submit = (but) =>
                {
					enableHitBox = !enableHitBox;
					but.label = !enableHitBox ? "HKTool.Debug.EnableHitBox".Get()
					: "HKTool.Debug.DisableHitBox".Get();
                }
			});
			if(HitBoxConfig == null)
            {
				HKToolMod.settings.DebugConfig.HitBoxConfig = new HitBoxConfig()
                {
                    colors = new List<HitBoxColor>()
                          {
                        new HitBoxColor()
                              {
                            layer = GlobalEnums.PhysLayers.PLAYER,
                            r = 0,
                            g = 1,
                            b = 0,
                            index = 0
                              },
                        new HitBoxColor()
                              {
                            layer = GlobalEnums.PhysLayers.HERO_ATTACK,
                            index=100
                              },
                        new HitBoxColor()
                              {
                            layer = GlobalEnums.PhysLayers.DEFAULT,
                            r = 0,
                            g = 1,
                            b = 0,
                            needPlayMakerFSMs = new List<string>()
                                  {
                                "damage_enemies"
                                  }
                              },
                        new HitBoxColor()
                              {
                            layer = GlobalEnums.PhysLayers.TERRAIN,
                            r = 1,
                            g = 1,
                            b = 0
                              },
                        new HitBoxColor()
                              {
                            layer = GlobalEnums.PhysLayers.DEFAULT,
                            needComponents = new List<string>()
                                  {
                                typeof(DamageHero).FullName
                                  },
                            r = 1,
                            g = 0,
                            b = 0,
                            index = -1
                              },
                        new HitBoxColor()
                        {
                            layer = GlobalEnums.PhysLayers.DEFAULT,
                            needComponents = new List<string>()
                            {
                                typeof(HealthManager).FullName
                            },
                            r = 1,
                            g = 0.7f,
                            b = 0.7f,
                            index = 0
                        }
                    }
                };
                
            }
			HitBoxConfig.colors = HitBoxConfig.colors.OrderBy(x => x.index).ToList();
			lastUpdate = Time.unscaledTime;
			//Menu.MenuManager.AddButton(new Menu.ButtonInfo()
			//{
			//	label = "刷新碰撞箱",
			//	submit = (_) => LoadHitBoxConfig()
			//});
			//LoadHitBoxConfig();
			RefreshHitBox();
            Modding.ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
		}

        private static void ModHooks_HeroUpdateHook()
        {
            if(Time.unscaledTime - lastUpdate > 0.5f)
            {
				RefreshHitBox();
            }
        }

        public static void RefreshHitBox()
		{
			foreach (var v in UnityEngine.Object.FindObjectsOfType<Collider2D>())
			{
				if (v.GetComponent<HitBoxScript>() == null) v.gameObject.AddComponent<HitBoxScript>();
			}
		}
		//public static void LoadHitBoxConfig()
  //      {
		//	Modding.Logger.Log("Update Hit Box");
		//	lastUpdate = Time.unscaledTime;
			
		//	string p = Path.Combine(HKDebugMod.ConfigPath, "HitBox.json");
  //          if (!File.Exists(p))
  //          {
		//		hitBoxConfig = new HitBoxConfig()
		//		{
		//			colors = new List<HitBoxColor>()
  //                  {
		//				new HitBoxColor()
  //                      {
		//					layer = GlobalEnums.PhysLayers.PLAYER,
		//					r = 0,
		//					g = 1,
		//					b = 0,
		//					index = 0
  //                      },
		//				new HitBoxColor()
  //                      {
		//					layer = GlobalEnums.PhysLayers.HERO_ATTACK,
		//					index=100
  //                      },
		//				new HitBoxColor()
  //                      {
		//					layer = GlobalEnums.PhysLayers.DEFAULT,
		//					r = 0,
		//					g = 1,
		//					b = 0,
		//					needPlayMakerFSMs = new List<string>()
  //                          {
		//						"damage_enemies"
  //                          }
  //                      },
		//				new HitBoxColor()
  //                      {
		//					layer = GlobalEnums.PhysLayers.TERRAIN,
		//					r = 1,
		//					g = 1,
		//					b = 0
  //                      },
		//				new HitBoxColor()
  //                      {
		//					layer = GlobalEnums.PhysLayers.DEFAULT,
		//					needComponents = new List<string>()
  //                          {
		//						typeof(DamageHero).FullName
  //                          },
		//					r = 1,
		//					g = 0,
		//					b = 0,
		//					index = -1
  //                      },
		//				new HitBoxColor()
		//				{
		//					layer = GlobalEnums.PhysLayers.DEFAULT,
		//					needComponents = new List<string>()
		//					{
		//						typeof(HealthManager).FullName
		//					},
		//					r = 1,
		//					g = 0.7f,
		//					b = 0.7f,
		//					index = 0
		//				}
		//			}
		//		};
		//		hitBoxConfig.colors = hitBoxConfig.colors.OrderBy(x => x.index).ToList();
		//		File.WriteAllText(p, JsonConvert.SerializeObject(hitBoxConfig, Formatting.Indented));
		//		return;
  //          }
		//	hitBoxConfig = JsonConvert.DeserializeObject<HitBoxConfig>(File.ReadAllText(p));
		//	hitBoxConfig.colors = hitBoxConfig.colors.OrderBy(x => x.index).ToList();
		//	RefreshHitBox();
  //      }
		public static LineRenderer SetupLineRenderer(Collider2D col,Material mat, LineRenderer line)
		{
			BoxCollider2D boxCollider2D;
			CircleCollider2D circleCollider2D;
			PolygonCollider2D polygonCollider2D;
			EdgeCollider2D edgeCollider2D;
			if ((boxCollider2D = (col as BoxCollider2D)) != null)
			{
				Vector2 vector = boxCollider2D.size / 2f;
				Vector2 vector2 = -vector;
				Vector2 a = new Vector2(vector.x, vector2.y);
				Vector2 a2 = -a;
				line.positionCount = 5;
				line.SetPositions(new Vector3[]
				{
					col.transform.TransformPoint(vector2 + boxCollider2D.offset),
					col.transform.TransformPoint(a2 + boxCollider2D.offset),
					col.transform.TransformPoint(vector + boxCollider2D.offset),
					col.transform.TransformPoint(a + boxCollider2D.offset),
					col.transform.TransformPoint(vector2 + boxCollider2D.offset)
				});
			}
			else if ((circleCollider2D = (col as CircleCollider2D)) != null)
			{
				//circleCollider2D.transform.position + circleCollider2D.offset;
				Vector3[] array = new Vector3[30];
				float num = 6.2831855f / (float)array.Length;
				for (int i = 0; i < array.Length - 1; i++)
				{
					float num2 = num * (float)i;
					float num3 = Mathf.Sin(num2);
					float num4 = Mathf.Cos(num2);
					array[i] = new Vector2((num4 - num3) * circleCollider2D.transform.localScale.x * circleCollider2D.radius, (num4 + num3) * circleCollider2D.transform.localScale.y * circleCollider2D.radius);
				}
				array[array.Length - 1] = array[0];
				line.positionCount = array.Length;
				line.SetPositions(array);
			}
			else if ((polygonCollider2D = (col as PolygonCollider2D)) != null)
			{
				Vector3[] array2 = new Vector3[polygonCollider2D.points.Length + 1];
				for (int j = 0; j < polygonCollider2D.points.Length; j++)
				{
					array2[j] = polygonCollider2D.transform.TransformPoint(polygonCollider2D.points[j]);
				}
				array2[array2.Length - 1] = array2[0];
				line.positionCount = array2.Length;
				line.SetPositions(array2);
			}
			else if ((edgeCollider2D = (col as EdgeCollider2D)) != null)
			{
				Vector3[] array3 = new Vector3[edgeCollider2D.points.Length];
				for (int k = 0; k < edgeCollider2D.points.Length; k++)
				{
					array3[k] = edgeCollider2D.transform.TransformPoint(edgeCollider2D.points[k]);
				}
				line.positionCount = array3.Length;
				line.SetPositions(array3);
			}
			return line;
		}
	}
}
