using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;

public class Scan_QRcode : MonoBehaviour
{
    [HideInInspector]
    public WebCamTexture myCam;//接收攝影機返回的圖片數據
    private BarcodeReader reader = new BarcodeReader();//ZXing的解碼
    private Result res;//儲存掃描後回傳的資訊
    private bool flag = true;//判斷掃描是否執行完畢

    private Coroutine scanCoroutine;

    public RawImage camImage;

    public AspectRatioFitter fit;

    void Start()
    {
        StartCoroutine(open_Camera());//開啟攝影機鏡頭
    }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    StartCoroutine(open_Camera());
        //}
        SetRatio();
        //print(myCam.name);
        if (myCam != null)//若有攝影機則將攝影機拍到的畫面畫出
        {
            if (myCam.isPlaying == true)//若攝影機已開啟
            {
                camImage.texture = myCam;
                //image.SetNativeSize();
                if (flag == true)//若掃描已執行完畢，則再繼續進行掃描，防止第一個掃描還沒結束就又再次掃描，造成嚴重的記憶體耗盡
                {
                    scanCoroutine = StartCoroutine(scan());
                }
            }

            //showIP.text = ipAddress;
        }
    }

    public IEnumerator open_Camera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);    //授權開啟鏡頭
        print(Application.HasUserAuthorization(UserAuthorization.WebCam));
        print(WebCamTexture.devices[0].name);
        print(WebCamTexture.devices.Length);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            //設置攝影機要攝影的區域
            myCam = new WebCamTexture(WebCamTexture.devices[0].name, 0, 0, 60);/* (攝影機名稱, 攝影機要拍到的寬度, 攝影機要拍到的高度, 攝影機的FPS) */
            myCam.Play();//開啟攝影機
            camImage.enabled = true;
        }
    }

    private IEnumerator scan()
    {
        flag = false;//掃描開始執行

        //Texture2D t2D = new Texture2D(Screen.width, Screen.height);//掃描後的影像儲存大小，越大會造成效能消耗越大，若影像嚴重延遲，請降低儲存大小。
        yield return new WaitForEndOfFrame();//等待攝影機的影像繪製完畢

        //t2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);//掃描的範圍，設定為整個攝影機拍到的影像，若影像嚴重延遲，請降低掃描大小。
        //t2D.Apply();//開始掃描
        if (reader == null)
            reader = new BarcodeReader();
        res = reader.Decode(myCam.GetPixels32(), myCam.width, myCam.height);//對剛剛掃描到的影像進行解碼，並將解碼後的資料回傳

        yield return new WaitForEndOfFrame();

        //若是掃描不到訊息，則res為null
        if (res != null)
        {
            Debug.Log(res.Text);//將解碼後的資料列印出來

             
            //DownloadZIP.URLtoDownload(res.Text);

            if (scanCoroutine != null)
                StopCoroutine(scanCoroutine);
            //uniWebView.Load(res.Text);
            //uniWebView.Show();

            //返回大廳
            //SceneManager.LoadScene("Lobby_Mobiles");

        }
        yield return new WaitForSeconds(.5f);
        flag = true;//掃描完畢
    }

    void OnDisable()
    {
        //當程式關閉時會自動呼叫此方法關閉攝影機
        if (myCam)
        {
            if (myCam.isPlaying)
                myCam.Stop();
            Destroy(myCam);
        }
        flag = true;

        if (scanCoroutine != null)
            StopCoroutine(scanCoroutine);
    }

    private void SetRatio()
    {
        if (myCam != null)//若有攝影機則將攝影機拍到的畫面畫出
        {
            if (myCam.isPlaying == true)//若攝影機已開啟
            {
                float ratio = (float)myCam.width / (float)myCam.height;
                fit.aspectRatio = ratio;

                //float scaleY = myCam.videoVerticallyMirrored ? -1f : 1f;
                //background.rectTransform.localScale = new Vector3 (1f, scaleY, 1f);    //非鏡像
                //camImage.rectTransform.localScale = new Vector3(-1f, scaleY, 1f);    //鏡像

                int orient = -myCam.videoRotationAngle;
                camImage.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
            }
        }
    }
}
