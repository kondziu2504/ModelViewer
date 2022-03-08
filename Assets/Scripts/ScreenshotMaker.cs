using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class ScreenshotMaker : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] Vector2Int resoulution = new Vector2Int(1920, 1080);

    public static string ScreenShotName(int width, int height)
    {
        string directory = Directory.GetCurrentDirectory() + "/Output";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        string Path(string filename) => string.Format("{0}/{1}.png", directory, filename);

        string filename = string.Format("{0}", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

        int screenshotInd = 0;
        while(true)
        {
            string filenameWithInd = filename + "_" + screenshotInd.ToString();
            if (File.Exists(Path(filenameWithInd)))
                screenshotInd++;
            else
            {
                filename = filenameWithInd;
                break;
            }
        }

        return Path(filename);
    }

    public void TakeHiResShot()
    {
        StartCoroutine(ScreenshotCoroutine());
    }

    private IEnumerator ScreenshotCoroutine()
    {
        yield return new WaitForEndOfFrame();

        Camera screenshotCamera = new GameObject("ScreenshotCamera").AddComponent<Camera>();
        screenshotCamera.CopyFrom(camera);

        RenderTexture renderTexture = new RenderTexture(resoulution.x, resoulution.y, 24);
        screenshotCamera.targetTexture = renderTexture;
        Texture2D screenShot = new Texture2D(resoulution.x, resoulution.y, TextureFormat.RGB24, false);
        screenshotCamera.Render();
        RenderTexture.active = renderTexture;

        screenShot.ReadPixels(new Rect(0, 0, resoulution.x, resoulution.y), 0, 0);
        yield return new WaitForEndOfFrame(); // Distribute "ReadPixels" and Writing to file across 2 frames

        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(resoulution.x, resoulution.y);

        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        Destroy(screenshotCamera.gameObject);

        File.WriteAllBytes(filename, bytes);
    }
}
