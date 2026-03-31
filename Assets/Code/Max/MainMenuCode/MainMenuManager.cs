using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // 1. 新遊戲按鈕執行的功能
    public void NewGame()
    {
        // 設定一個標記 (0 代表不讀取存檔)，並寫入硬碟
        PlayerPrefs.SetInt("LoadFromSave", 0);
        PlayerPrefs.Save();

        // 載入遊戲場景
        SceneManager.LoadScene("Test0324");
    }

    // 2. 讀取遊戲按鈕執行的功能
    public void LoadGame()
    {
        // 先檢查硬碟裡到底有沒有存檔 (HasSaveData 是我們在存檔點設定的)
        if (PlayerPrefs.GetInt("HasSaveData", 0) == 1)
        {
            // 設定一個標記 (1 代表要讀取存檔)，並寫入硬碟
            PlayerPrefs.SetInt("LoadFromSave", 1);
            PlayerPrefs.Save();

            // 載入遊戲場景
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            // 如果沒有存檔，就在控制台印出提示 (遊戲畫面不會有反應)
            Debug.Log("目前沒有任何存檔紀錄！");
        }
    }

    // 3. 結束遊戲按鈕執行的功能
    public void QuitGame()
    {
        Debug.Log("遊戲已關閉！");
        Application.Quit();
    }
}
