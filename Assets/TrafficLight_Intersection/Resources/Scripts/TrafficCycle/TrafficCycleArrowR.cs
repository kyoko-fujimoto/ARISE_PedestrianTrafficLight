using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Consts;

/*	
 *	右折矢印の信号サイクルスクリプト
 */

public class TrafficCycleArrowR : TrafficCycle {

    //灯器用スクリプト
    private TrafficLightArrowSetting[, ] tlas1, tlas2;

    //灯火のゲームオブジェクト
    private GameObject[,] lightA1, lightA2;

	//車灯用の時間
	//方向1青時間、方向2青時間、方向1矢印時間、方向2矢印時間、黄時間(1)、黄時間(2)、全赤時間
	public float greenTime1, greenTime2, arrowTime1, arrowTime2, yellowTime, yellowTime2, allRedTime;
	//歩灯用の時間
	//方向1青時間、方向2青時間、方向1点滅回数、方向2点滅回数
	public float pGreenTime1, pGreenTime2;
	public int pBlinkTimes1, pBlinkTimes2;

	//開始時のステップ(1 or 2)
	public int startStep = 1;
    //オフセット（時間のずれ）
    public float offset = 0;


    //経過時間ゲージをオンにするかどうか
    //public bool timeGauge = false;

    //音響をオンにするかどうか
    public bool audioOn = false;
    private GameObject[] audioDevice1, audioDevice2;
	public AudioClip audioClip1, audioClip2;



	// Use this for initialization
	void Start () {
		
		//信号方向の設定
		GameObject[] tempDirection;
		tempDirection = GameObject.FindGameObjectsWithTag("Direction1");
		direction1 = getChildObjects(tempDirection);
		tempDirection = GameObject.FindGameObjectsWithTag("Direction2");
		direction2 = getChildObjects(tempDirection);
		tempDirection = GameObject.FindGameObjectsWithTag("DirectionP1");
		directionP1 = getChildObjects(tempDirection);
		tempDirection = GameObject.FindGameObjectsWithTag("DirectionP2");
		directionP2 = getChildObjects(tempDirection);


		//各灯器スクリプトの設定
		tls1 = new TrafficLightSetting[direction1.Length];
		tls2 = new TrafficLightSetting[direction2.Length];
		for (int i = 0; i < tls1.Length; i++) {
			tls1[i] = direction1 [i].GetComponent<TrafficLightSetting> ();
		}
		for (int i = 0; i < tls2.Length; i++) {
			tls2[i] = direction2 [i].GetComponent<TrafficLightSetting> ();
		}
		tlps1 = new TrafficLightPSetting[directionP1.Length];
		tlps2 = new TrafficLightPSetting[directionP2.Length];
		for (int i = 0; i < tlps1.Length; i++) {
			tlps1[i] = directionP1 [i].GetComponent<TrafficLightPSetting> ();
		}
		for (int i = 0; i < tlps2.Length; i++) {
			tlps2[i] = directionP2 [i].GetComponent<TrafficLightPSetting> ();
		}
		tlas1 = new TrafficLightArrowSetting[direction1.Length, 1];
		tlas2 = new TrafficLightArrowSetting[direction2.Length, 1];
		for (int i = 0; i < tlas1.GetLength(0); i++) {
			tlas1[i, 0] = direction1 [i].transform.Find ("TrafficLightArrow_R").GetComponent<TrafficLightArrowSetting> ();
		}
		for (int i = 0; i < tlas2.GetLength(0); i++) {
			tlas2[i, 0] = direction2 [i].transform.Find ("TrafficLightArrow_R").GetComponent<TrafficLightArrowSetting> ();
		}


		//各灯火の設定
		light1 = new GameObject[direction1.Length, 3];
		light2 = new GameObject[direction2.Length, 3];
		lightA1 = new GameObject[direction1.Length, 1]; 
		lightA2 = new GameObject[direction2.Length, 1]; 
		if (directionP1.Length != 0) {
			lightP1 = new GameObject[directionP1.Length, 2];
		}
		if (directionP2.Length != 0) {
			lightP2 = new GameObject[directionP2.Length, 2];
		}
        //車灯（0:青灯火, 1:黄灯火, 2:赤灯火）のセッティング
        for (int i = 0; i < light1.GetLength(0); i++) {
            light1[i, 0] = tls1[i].SetupLightG(direction1[i], 0);
            light1[i, 1] = tls1[i].SetupLightY(direction1[i], 1);
            light1[i, 2] = tls1[i].SetupLightR(direction1[i], 2);
        }
        for (int i = 0; i < light2.GetLength(0); i++) {
            light2[i, 0] = tls2[i].SetupLightG(direction2[i], 0);
            light2[i, 1] = tls2[i].SetupLightY(direction2[i], 1);
            light2[i, 2] = tls2[i].SetupLightR(direction2[i], 2);
        }
        //矢印灯(lightA1, lightA2) 0:右折
        for (int i = 0; i < lightA1.GetLength(0); i++) {
			if (tlas1 [i, 0].lightType == 1) {
				lightA1 [i, 0] = direction1 [i].transform.Find ("TrafficLightArrow_R/TLightArrow_R").gameObject;
			} else if (tlas1 [i, 0].lightType == 0 || tlas1 [i, 0].lightType == 2){
				lightA1 [i, 0] = direction1 [i].transform.Find ("TrafficLightArrow_R/TLightGroupArrow_R/PointLightA").gameObject;
			}
		}
		for (int i = 0; i < lightA2.GetLength(0); i++) {
			if (tlas2 [i, 0].lightType == 1) {
				lightA2 [i, 0] = direction2 [i].transform.Find ("TrafficLightArrow_R/TLightArrow_R").gameObject;
			} else if(tlas2 [i, 0].lightType == 0  || tlas2 [i, 0].lightType == 2){
				lightA2 [i, 0] = direction2 [i].transform.Find ("TrafficLightArrow_R/TLightGroupArrow_R/PointLightA").gameObject;
			}
		}

        //歩灯の灯火（0:赤灯火, 1:青灯火）のセッティング
        if (directionP1.Length != 0) {
            for (int i = 0; i < lightP1.GetLength(0); i++) {
                lightP1[i, 0] = tlps1[i].SetupLightPR(directionP1[i]);
                lightP1[i, 1] = tlps1[i].SetupLightPG(directionP1[i]);
            }
        }
        if (directionP2.Length != 0) {
            for (int i = 0; i < lightP2.GetLength(0); i++) {
                lightP2[i, 0] = tlps2[i].SetupLightPR(directionP2[i]);
                lightP2[i, 1] = tlps2[i].SetupLightPG(directionP2[i]);
            }
        }


        //青歩灯の時間に合わせて青車灯の時間を変更
        float minimumTime1 = 0f, minimumTime2 = 0f;
		if (directionP1.Length != 0) {
			minimumTime1 = pGreenTime1 + CONSTS.BLINK_TIME * 2 * pBlinkTimes1 + CONSTS.CHANGE_TIME;
			if (greenTime1 < minimumTime1) {
				greenTime1 = minimumTime1;
			}
		} else if (greenTime1 <= 0) {
			greenTime1 = CONSTS.G_TIME;
		}
		if (directionP2.Length != 0) {
			minimumTime2 = pGreenTime2 + CONSTS.BLINK_TIME * 2 * pBlinkTimes2 + CONSTS.CHANGE_TIME;
			if (greenTime2 < minimumTime2) {
				greenTime2 = minimumTime2;
			}
		} else if (greenTime2 <= 0) {
			greenTime2 = CONSTS.G_TIME;
		}

		//各点灯時間未設定(0以下を指定)時はデフォルト値に変更
		if (arrowTime1 <= 0) {
			arrowTime1 = 3f;
		}
		if (yellowTime <= 0) {
			yellowTime = CONSTS.Y_TIME;
		}
		if (yellowTime2 <= 0) {
			yellowTime2 = CONSTS.Y_TIME;
		}
		if (allRedTime <= 0) {
			allRedTime = CONSTS.ALLR_TIME;
		}

		//赤時間(1) ＝ 青時間(2) + 矢印時間(2) + 黄時間(1)(2)  + 全赤時間 * 2
		redTime1 = greenTime2 + arrowTime2 + yellowTime + yellowTime2 + allRedTime * 2;
		//赤時間(2) ＝ 青時間(1) + 矢印時間(1) + 黄時間(1)(2) + 全赤時間
		redTime2 = greenTime1 + arrowTime1 + yellowTime + yellowTime2 + allRedTime;

		//歩灯赤時間(1)
		if (directionP1.Length != 0) {
			pRedTime1 = redTime1 + (greenTime1 - minimumTime1 + CONSTS.CHANGE_TIME) + arrowTime1 + yellowTime + yellowTime2;
		}
		//歩灯赤時間(2)
		if (directionP2.Length != 0) {
			pRedTime2 = redTime2  + (greenTime2 - minimumTime2 + CONSTS.CHANGE_TIME) + arrowTime2 + yellowTime + yellowTime2 + allRedTime;
		}

		//1サイクルの時間
		allTime = greenTime1 + arrowTime1 +  yellowTime + yellowTime2 + redTime1;
		Debug.Log (this.transform.name + " ：" + allTime + "秒");

		//経過時間ゲージ関連
		restRedTime2 = pRedTime2 - redTime2;


		//音響装置の設定
		if (audioOn) {
			GameObject[] tempAudioDevice;
			tempAudioDevice = GameObject.FindGameObjectsWithTag("AudioDevice1");
			audioDevice1 = getChildObjects(tempAudioDevice);
			tempAudioDevice = GameObject.FindGameObjectsWithTag("AudioDevice2");
			audioDevice2 = getChildObjects(tempAudioDevice);

			for (int j = 0; j < audioDevice1.Length; j++) {
				audioDevice1 [j].GetComponent<AudioSource>().clip = audioClip1;
			}
			for (int j = 0; j < audioDevice2.Length; j++) {
				audioDevice2 [j].GetComponent<AudioSource>().clip = audioClip2;
			}
		}


		//サイクル開始時の状態
		if (startStep == 1) {			//南北方向が青の場合
			cTime = 0;
			cStep1 = 0;
			cStep2 = 2;
			if (audioOn) {
				for (int j = 0; j < audioDevice1.Length; j++) {
					audioDevice1 [j].GetComponent<AudioSource>().Play();
				}
			}
		} else if (startStep == 2) {	//東西方向が青の場合
			cTime = redTime2;
			cStep1 = 2;
			cStep2 = 0;
			if (audioOn) {
				for (int j = 0; j < audioDevice2.Length; j++) {
					audioDevice2 [j].GetComponent<AudioSource>().Play();
				}
			}
		}
        //オフセット
        cTime = cTime + offset;
    }

	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate(){
		//方向１車灯の信号サイクル
		for(int i = 0; i < light1.GetLength(0); i++){
			if (cTime < greenTime1) {
				//青信号に
				tls1 [i].SettingLightG (light1 [i, 0], true);
				tls1 [i].SettingLightY (light1 [i, 1], false);
				tls1 [i].SettingLightR (light1 [i, 2], false);
				tlas1 [i, 0].SettingLightA (lightA1 [i, 0], false);
			} else if (cTime < (greenTime1 + yellowTime)) {
				//黄信号に
				tls1 [i].SettingLightG (light1 [i, 0], false);
				tls1 [i].SettingLightY (light1 [i, 1], true);
				tls1 [i].SettingLightR (light1 [i, 2], false);
				tlas1 [i, 0].SettingLightA (lightA1 [i, 0], false);
			} else if (cTime < (greenTime1 + arrowTime1 + yellowTime)) {
				//赤+矢印信号に
				tls1 [i].SettingLightG (light1 [i, 0], false);
				tls1 [i].SettingLightY (light1 [i, 1], false);
				tls1 [i].SettingLightR (light1 [i, 2], true);
				tlas1 [i, 0].SettingLightA (lightA1 [i, 0], true);
			} else if (cTime < (greenTime1 + arrowTime1 + yellowTime + yellowTime2)) {
				//黄信号に
				tls1 [i].SettingLightG (light1 [i, 0], false);
				tls1 [i].SettingLightY (light1 [i, 1], true);
				tls1 [i].SettingLightR (light1 [i, 2], false);
				tlas1 [i, 0].SettingLightA (lightA1 [i, 0], false);
			} else if (cTime < allTime) {
				//赤信号に
				tls1 [i].SettingLightG (light1 [i, 0], false);
				tls1 [i].SettingLightY (light1 [i, 1], false);
				tls1 [i].SettingLightR (light1 [i, 2], true);
				tlas1 [i, 0].SettingLightA (lightA1 [i, 0], false);
			} else {
				//青信号に
				tls1 [i].SettingLightG (light1 [i, 0], true);
				tls1 [i].SettingLightY (light1 [i, 1], false);
				tls1 [i].SettingLightR (light1 [i, 2], false);
				tlas1 [i, 0].SettingLightA (lightA1 [i, 0], false);
			}
		}

		//方向２車灯の信号サイクル
		for(int i = 0; i < light2.GetLength(0); i++){
			if (cTime < redTime2) {
				//赤信号に
				tls2[i].SettingLightG (light2 [i, 0], false);
				tls2[i].SettingLightY (light2 [i, 1], false);
				tls2[i].SettingLightR (light2 [i, 2], true);
				tlas2[i, 0].SettingLightA (lightA2[i, 0], false);
			} else if (cTime < (redTime2 + greenTime2)) {
				//青信号に
				tls2[i].SettingLightG (light2 [i, 0], true);
				tls2[i].SettingLightY (light2 [i, 1], false);
				tls2[i].SettingLightR (light2 [i, 2], false);
				tlas2[i, 0].SettingLightA (lightA2[i, 0], false);
			} else if (cTime < (redTime2 + greenTime2 + yellowTime)) {
				//黄信号に
				tls2[i].SettingLightG (light2 [i, 0], false);
				tls2[i].SettingLightY (light2 [i, 1], true);
				tls2[i].SettingLightR (light2 [i, 2], false);
				tlas2[i, 0].SettingLightA (lightA2[i, 0], false);
			} else if (cTime < (redTime2 + greenTime2 + arrowTime2 + yellowTime)) {
				//赤+矢印信号に
				tls2[i].SettingLightG (light2 [i, 0], false);
				tls2[i].SettingLightY (light2 [i, 1], false);
				tls2[i].SettingLightR (light2 [i, 2], true);
				tlas2[i, 0].SettingLightA (lightA2 [i, 0], true);
			} else if (cTime < (redTime2 + greenTime2 + arrowTime2 + yellowTime + yellowTime2)) {
				//黄信号に
				tls2[i].SettingLightG (light2 [i, 0], false);
				tls2[i].SettingLightY (light2 [i, 1], true);
				tls2[i].SettingLightR (light2 [i, 2], false);
				tlas2[i, 0].SettingLightA (lightA2 [i, 0], false);
			} else {
				//赤信号に
				tls2[i].SettingLightG (light2 [i, 0], false);
				tls2[i].SettingLightY (light2 [i, 1], false);
				tls2[i].SettingLightR (light2 [i, 2], true);
				tlas2[i, 0].SettingLightA (lightA2[i, 0], false);
			} 
		}

		//方向１歩灯の信号サイクル
		if (directionP1.Length != 0) {
			if (cStep1 == 0) {
				if (cTime < pGreenTime1) {
					for (int i = 0; i < lightP1.GetLength (0); i++) {
						//青信号に
						tlps1[i].SettingLightPR (lightP1 [i, 0], false);
						tlps1[i].SettingLightPG (lightP1 [i, 1], true);
					}
				} else {
					cStep1 = 1;		//点滅ステップへ
					if (audioOn) {
						for (int j = 0; j < audioDevice1.Length; j++) {
                            if (audioDevice1[j].GetComponent<AudioSource>().isPlaying) {
                                audioDevice1[j].GetComponent<AudioSource>().Stop();
                            }
						}
					}
				}

			} else if (cStep1 == 1) {	//点滅ステップ
				for (int i = 0; i < lightP1.GetLength (0); i++) {
					if (cPTime >= (CONSTS.BLINK_TIME * blinkCnt * 2) && cPTime < (CONSTS.BLINK_TIME * (blinkCnt*2+1))) {
						//消灯
						tlps1[i].SettingLightPG (lightP1 [i, 1], false);
					} else {
						//青
						tlps1[i].SettingLightPG (lightP1 [i, 1], true);
					}
				}

				//点滅回数の更新
				if(cPTime >= CONSTS.BLINK_TIME * (blinkCnt*2+2)){
					blinkCnt++;
				}
				cPTime += Time.deltaTime;

				if (blinkCnt == pBlinkTimes1) {
					cPTime = 0f;
					blinkCnt = 0;
					cStep1 = 2;
				}

			} else if (cStep1 == 2) {	//赤信号ステップ
				if (cTime > 1) {
					for (int i = 0; i < lightP1.GetLength (0); i++) {
						//赤信号に
						tlps1[i].SettingLightPR (lightP1 [i, 0], true);
						tlps1[i].SettingLightPG (lightP1 [i, 1], false);
					}
				} else {
					cStep1 = 0;
					if (audioOn) {
						for (int j = 0; j < audioDevice1.Length; j++) {
                            if (!audioDevice1[j].GetComponent<AudioSource>().isPlaying) {
                                audioDevice1[j].GetComponent<AudioSource>().Play();
                            }
						}
					}
				}
			}
		}

		//方向2歩灯の信号サイクル
		if (directionP2.Length != 0) {
			if (cStep2 == 0) {
				if (cTime >= redTime2 && cTime < redTime2 + pGreenTime2) {
					for (int i = 0; i < lightP2.GetLength (0); i++) {
						//青信号に
						tlps2[i].SettingLightPR (lightP2 [i, 0], false);
						tlps2[i].SettingLightPG (lightP2 [i, 1], true);
					} 
				} else if (cTime >= redTime2 + pGreenTime2){
					cStep2 = 1;		//点滅ステップへ	
					if (audioOn) {
						for (int j = 0; j < audioDevice2.Length; j++) {
                            if (audioDevice2[j].GetComponent<AudioSource>().isPlaying) {
                                audioDevice2[j].GetComponent<AudioSource>().Stop();
                            }
						}
					}
				}

			} else if (cStep2 == 1) {	//点滅ステップ

				for (int i = 0; i < lightP2.GetLength (0); i++) {
					if (cPTime >= (CONSTS.BLINK_TIME * blinkCnt * 2) && cPTime < (CONSTS.BLINK_TIME * (blinkCnt*2+1))) {
						//消灯
						tlps2[i].SettingLightPG (lightP2 [i, 1], false);
					} else {
						//青
						tlps2[i].SettingLightPG (lightP2 [i, 1], true);
					}
				}

				//点滅回数の更新
				if(cPTime >= CONSTS.BLINK_TIME * (blinkCnt*2+2)){
					blinkCnt++;
				}
				cPTime += Time.deltaTime;

				if (blinkCnt == pBlinkTimes2) {
					cPTime = 0f;
					blinkCnt = 0;
					cStep2 = 2;
				}

			} else if (cStep2 == 2) {	//赤信号ステップ
				if (cTime > redTime2 + pGreenTime2 || cTime < redTime2) {
					for (int i = 0; i < lightP2.GetLength (0); i++) {
						//赤信号に
						tlps2[i].SettingLightPR (lightP2 [i, 0], true);
						tlps2[i].SettingLightPG (lightP2 [i, 1], false);
					}
				} else {
					cStep2 = 0;
					if (audioOn) {
						for (int j = 0; j < audioDevice2.Length; j++) {
                            if (!audioDevice2[j].GetComponent<AudioSource>().isPlaying) {
                                audioDevice2[j].GetComponent<AudioSource>().Play();
                            }
						}
					}
				}
			}
		}

        //方向1の経過時間ゲージ
        if (directionP1.Length != 0) {
            for (int i = 0; i < directionP1.GetLength(0); i++) {
                if (tlps1[i].timeGaugeType != 0) {
                    if (tlps1[i].lightType[0] == 1) {
                        tlps1[i].SettingTimeGaugeR(cTime, allTime - pRedTime1, 0, pRedTime1);
                    }
                    if (tlps1[i].lightType[1] == 1) {
                        tlps1[i].SettingTimeGaugeG(cTime, 0, pGreenTime1);
                    }
                }
            }
        }
        //方向2の経過時間ゲージ
        if (directionP2.Length != 0) {
            for (int i = 0; i < directionP2.GetLength(0); i++) {
                if (tlps2[i].timeGaugeType != 0) {
                    if (tlps2[i].lightType[0] == 1) {
                        tlps2[i].SettingTimeGaugeR(cTime, allTime - restRedTime2, redTime2, pRedTime2);
                    }
                    if (tlps2[i].lightType[1] == 1) {
                        tlps2[i].SettingTimeGaugeG(cTime, redTime2, pGreenTime2);
                    }
                }
            }
        }
        

		//サイクル時間のリセットと更新
		if (cTime >= allTime) {
			cTime = 0f;
		} else {
			cTime += Time.deltaTime;
		}
	}


}
