using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
界面上有三个输入框，分别对应 X,Y,Z 的值，请实现 {@link Q1.onGenerateBtnClick} 函数，生成一个 10 × 10 的可控随机矩阵，并显示到界面上，矩阵要求如下：
1. {@link COLORS} 中预定义了 5 种颜色
2. 每个点可选 5 种颜色中的 1 种
3. 按照从左到右，从上到下的顺序，依次为每个点生成颜色，(0, 0)为左上⻆点，(9, 9)为右下⻆点，(0, 9)为右上⻆点
4. 点(0, 0)随机在 5 种颜色中选取
5. 其他各点的颜色计算规则如下，设目标点坐标为(m, n）：
    a. (m, n - 1)所属颜色的概率为基准概率加 X%
    b. (m - 1, n)所属颜色的概率为基准概率加 Y%
    c. 如果(m, n - 1)和(m - 1, n)同色，则该颜色的概率为基准概率加 Z%
    d. 其他颜色平分剩下的概率
*/

public class Q1 : MonoBehaviour
{
    private static readonly Color[] COLORS = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(1f, 0.5f, 0f) // Orange
    };

    // 每个格子的大小
    private const float GRID_ITEM_SIZE = 75f;

    [SerializeField]
    private InputField xInputField = null;

    [SerializeField]
    private InputField yInputField = null;

    [SerializeField]
    private InputField zInputField = null;

    [SerializeField]
    private Transform gridRootNode = null;

    [SerializeField]
    private GameObject gridItemPrefab = null;

    public void OnGenerateBtnClick()
    {
        // Parse X, Y, Z values from input fields
        float x = 0f, y = 0f, z = 0f;
        float.TryParse(xInputField.text, out x);
        float.TryParse(yInputField.text, out y);
        float.TryParse(zInputField.text, out z);

        // Convert percentages to probability adjustments
        x /= 100f;
        y /= 100f;
        z /= 100f;

        // Create 10x10 color matrix
        Color[,] matrix = new Color[10, 10];

        // Point (0,0) - random color
        matrix[0, 0] = COLORS[Random.Range(0, COLORS.Length)];

        // Fill the rest row by row, left to right, top to bottom
        for (int m = 0; m < 10; m++)
        {
            for (int n = 0; n < 10; n++)
            {
                if (m == 0 && n == 0) continue;

                // Calculate probabilities for each color
                float[] probabilities = new float[COLORS.Length];
                float baseProb = 1f / COLORS.Length;

                for (int i = 0; i < COLORS.Length; i++)
                {
                    probabilities[i] = baseProb;
                }

                // Get neighbor colors
                Color colorAbove = n > 0 ? matrix[m, n - 1] : Color.clear;
                Color colorLeft = m > 0 ? matrix[m - 1, n] : Color.clear;

                // Find indices of neighbor colors
                int aboveColorIdx = -1, leftColorIdx = -1;
                for (int i = 0; i < COLORS.Length; i++)
                {
                    if (colorAbove == COLORS[i]) aboveColorIdx = i;
                    if (colorLeft == COLORS[i]) leftColorIdx = i;
                }

                // Apply probability adjustments
                if (aboveColorIdx >= 0)
                {
                    probabilities[aboveColorIdx] += x;
                }
                if (leftColorIdx >= 0)
                {
                    probabilities[leftColorIdx] += y;
                }
                if (aboveColorIdx >= 0 && leftColorIdx >= 0 && aboveColorIdx == leftColorIdx)
                {
                    probabilities[aboveColorIdx] += z;
                }

                // Normalize probabilities
                float total = 0f;
                for (int i = 0; i < COLORS.Length; i++) total += probabilities[i];
                for (int i = 0; i < COLORS.Length; i++) probabilities[i] /= total;

                // Select color based on probability
                float rand = Random.value;
                float cumulative = 0f;
                for (int i = 0; i < COLORS.Length; i++)
                {
                    cumulative += probabilities[i];
                    if (rand <= cumulative)
                    {
                        matrix[m, n] = COLORS[i];
                        break;
                    }
                }
            }
        }

        // Display matrix on UI
        DisplayMatrix(matrix);
    }

    private void DisplayMatrix(Color[,] matrix)
    {
        // Clear existing grid items
        foreach (Transform child in gridRootNode)
        {
            Destroy(child.gameObject);
        }

        // Get GridLayoutGroup if exists
        GridLayoutGroup gridLayout = gridRootNode.GetComponent<GridLayoutGroup>();
        float cellSizeX = gridLayout != null ? gridLayout.cellSize.x : GRID_ITEM_SIZE;
        float cellSizeY = gridLayout != null ? gridLayout.cellSize.y : GRID_ITEM_SIZE;
        float spacingX = gridLayout != null ? gridLayout.spacing.x : 0;
        float spacingY = gridLayout != null ? gridLayout.spacing.y : 0;

        // Create grid items
        for (int m = 0; m < 10; m++)
        {
            for (int n = 0; n < 10; n++)
            {
                GameObject item = Instantiate(gridItemPrefab, gridRootNode);
                item.GetComponent<Image>().color = matrix[m, n];

                // Set position based on grid coordinates
                float posX = n * (cellSizeX + spacingX) - 400 + 60;
                float posY = -m * (cellSizeY + spacingY) + 400 - 60;
                item.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);
            }
        }
    }
}
