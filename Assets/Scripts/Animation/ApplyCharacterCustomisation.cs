using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class colorSwap
{
    public Color fromColor;
    public Color toColor;

    public colorSwap(Color fromColor, Color toColor)
    {
        this.fromColor = fromColor;
        this.toColor = toColor;
    }
}

/// <summary>
/// 我们用素材纹理（人物主体纹理 + 其他服装纹理），依据选定的服装设置，更新人物主体纹理以及其他服装纹理，最后再把更新后的纹理合并到主体纹理
/// </summary>
public class ApplyCharacterCustomisation : MonoBehaviour
{
    // 输入纹理，其实就是要准备的素材，我们利用这些素材拼接起来想要的纹理
    [Header("素材纹理")]
    [SerializeField] private Texture2D maleFarmerBaseTexture = null;
    [SerializeField] private Texture2D femaleFarmerBaseTexture = null;
    [SerializeField] private Texture2D shirtsBaseTexture = null;
    [SerializeField] private Texture2D hairBaseTexture = null;
    [SerializeField] private Texture2D hatsBaseTexture = null;
    [SerializeField] private Texture2D adornmentsBaseTexture = null;
    private Texture2D farmerBaseTexture; //先根据选择的性别，将maleFarmerBaseTexture或femaleFarmerBaseTexture纹理赋值给它

    // 输出纹理
    [Header("动画使用的输出纹理")]
    //输出纹理的成品，这里只需赋值一个和farmerBaseTexture同样大小的纹理即可，后续会进一步改变
    [SerializeField] private Texture2D farmerBaseCustomised = null; 
    [SerializeField] private Texture2D hairCustomised = null;
    [SerializeField] private Texture2D hatsCustomised = null;
    private Texture2D farmerBaseShirtsUpdated; 
    private Texture2D selectedShirt; //存放选择后的衬衫纹理
    private Texture2D farmerBaseAdornmentsUpdated;
    private Texture2D selectedAdornment;

    // 选择性别，但本项目其实并没有女性素材
    [Header("选择性别: 0=Male, 1=Female")] [Range(0, 1)] [SerializeField]
    private int inputSex = 0;
    
    // Select Hair Style
    [Header("选择头发样式")] [Range(0, 2)] [SerializeField]
    private int inputHairStyleNo = 0;
    // 选择头发颜色
    [SerializeField] private Color inputHairColor = Color.black;
    
    // Select Skin Type
    [Header("选择皮肤类型")] [Range(0, 3)] [SerializeField]
    private int inputSkinType = 0;
    
    // Select Hat Style
    [Header("选择帽子类型")] [Range(0, 1)] [SerializeField]
    private int inputHatStyleNo = 0;
    
    // 选择shirt样式
    [Header("选择shirt样式")] [Range(0, 1)] [SerializeField]
    private int inputShirtStyleNo = 0;
    
    // 选择裤子颜色
    [Header("选择裤子颜色")][SerializeField] private Color inputTrouserColor = Color.blue;
    
    // Select Adornments Style
    [Header("选择装饰样式")] [Range(0, 2)] [SerializeField]
    private int inputAdornmentsStyleNo = 0;

    [Header("注意")]
    [Tooltip("这里人物肤色似乎更改不了，绿色衬衫也不能覆盖手臂。我利用源码查看，发现也有同样的问题，或许是因为版本")]
    public bool 鼠标放上提示;
    
    //指明面向的方向
    private Facing[,] bodyFacingArray;
    //对应的偏移
    private Vector2Int[,] bodyShirtOffsetArray;

    private Vector2Int[,] bodyAdornmentsOffsetArray;


    #region 指明纹理的大小
    
    //身体纹理的行和列
    private int bodyRows = 21;
    private int bodyColumns = 6;
    
    //farmer的像素大小
    private int farmerSpriteWidth = 16;
    private int farmerSpriteHeight = 32;
    
    //一组shirt的像素大小，9*36
    private int shirtTextureWidth = 9;
    private int shirtTextureHeight = 36;
    
    //单个shirt的像素大小，9*9
    private int shirtSpriteWidth = 9;
    private int shirtSpriteHeight = 9;
    private int shirtStylesInSpriteWidth = 16;

    private int hairTextureWidth = 16;
    private int hairTextureHeight = 96;
    private int hairStylesInSpriteWidth = 8;

    private int hatTextureWidth = 20;
    private int hatTextureHeight = 80;
    private int hatStylesInSpriteWidth = 12;

    private int adornmentsTextureWidth = 16;
    private int adornmentsTextureHeight = 32;
    private int adornmentsStylesInSpriteWidth = 8;
    private int adornmentsSpriteWidth = 16;
    private int adornmentsSpriteHeight = 16;
    
    #endregion
    
    
    private List<colorSwap> colorSwapList;

    //基础纹理中手臂的颜色，这个颜色将被替换
    private Color32 armTargetColor1 = new Color32(77, 13, 13, 255);  // darkest
    private Color32 armTargetColor2 = new Color32(138, 41, 41, 255); // next darkest
    private Color32 armTargetColor3 = new Color32(172, 50, 50, 255); // lightest
    
    //基础纹理中皮肤的颜色，这个颜色将被替换
    private Color32 skinTargetColor1 = new Color32(145, 117, 90, 255);  // darkest
    private Color32 skinTargetColor2 = new Color32(204, 155, 108, 255);  // next darkest
    private Color32 skinTargetColor3 = new Color32(207, 166, 128, 255);  // next darkest
    private Color32 skinTargetColor4 = new Color32(238, 195, 154, 255);  // lightest

    private void Awake()
    {
        // Initialise color swap list
        colorSwapList = new List<colorSwap>();

        // Process Customisation
        ProcessCustomisation();
    }

    
    #region Awake中调用的函数
    private void ProcessCustomisation()
    {
        //处理性别，先根据性别更新到farmerBaseTexture，再将其像素传给farmerBaseCustomised
        ProcessGender();

        //处理选择的衬衫
        ProcessShirt();

        //处理手臂
        ProcessArms();
        
        //处理裤子
        ProcessTrousers();

        ProcessHair();

        ProcessSkin();

        ProcessHat();

        ProcessAdornments();
        
        MergeCustomisations();
    }

    private void ProcessGender()
    {
        //根据性别设置farmerBaseTexture
        if (inputSex == 0)
        {
            farmerBaseTexture = maleFarmerBaseTexture;
        }
        else if (inputSex == 1)
        {
            farmerBaseTexture = femaleFarmerBaseTexture;
        }

        //返回纹理的整个Mip级别的像素颜色数组。返回的数组是一个平展2D数组，其中像素是从左到右、 从下到上排列（即，逐行排列）
        Color[] farmerBasePixels = farmerBaseTexture.GetPixels();

        //该函数接收一个颜色数组， 然后更改纹理的整个 Mip 级别的像素颜色
        farmerBaseCustomised.SetPixels(farmerBasePixels);
        //调用 Apply 可实际将更改的像素 上传到显卡
        farmerBaseCustomised.Apply();
    }

    private void ProcessShirt()
    {
        // 为bodyFacingArray分配内存
        bodyFacingArray = new Facing[bodyColumns, bodyRows];
        // 设置bodyFacingArray，填充每一个元素的值
        PopulateBodyFacingArray();
        
        // 为bodyShirtOffsetArray分配内存
        bodyShirtOffsetArray = new Vector2Int[bodyColumns, bodyRows];
        // 设置bodyShirtOffsetArray，填充每一个元素的值
        PopulateBodyShirtOffsetArray();

        
        // 根据选择的衬衫样式，设置纹理
        AddShirtToTexture(inputShirtStyleNo);

        // Apply shirt texture to base
        ApplyShirtTextureToBase();
    }

    private void ProcessArms()
    {
        // Get arm pixels to recolor
        Color[] farmerPixelsToRecolour = farmerBaseTexture.GetPixels(0, 0, 288, farmerBaseTexture.height);

        // Populate Arm Color Swap List
        PopulateArmColorSwapList();

        // Change arm colours
        ChangePixelColors(farmerPixelsToRecolour, colorSwapList);

        // Set recoloured pixels
        farmerBaseCustomised.SetPixels(0, 0, 288, farmerBaseTexture.height, farmerPixelsToRecolour);

        // Apply texture changes
        farmerBaseCustomised.Apply();
    }

    private void ProcessTrousers()
    {
        // Get trouser pixels to recolor
        Color[] farmerTrouserPixels = farmerBaseTexture.GetPixels(288, 0, 96, farmerBaseTexture.height);

        // Change trouser colours
        TintPixelColors(farmerTrouserPixels, inputTrouserColor);

        // Set changed trouser pixels
        farmerBaseCustomised.SetPixels(288, 0, 96, farmerBaseTexture.height, farmerTrouserPixels);

        // Apply texture changes
        farmerBaseCustomised.Apply();

    }
    
    private void ProcessHair()
    {
        // Create Selected Hair Texture
        AddHairToTexture(inputHairStyleNo);

        // Get hair pixels to recolor
        Color[] farmerSelectedHairPixels = hairCustomised.GetPixels();

        // Tint hair pixels
        TintPixelColors(farmerSelectedHairPixels, inputHairColor);

        hairCustomised.SetPixels(farmerSelectedHairPixels);
        hairCustomised.Apply();
    }
    
    private void ProcessSkin()
    {
        // Get skin pixels to recolor
        Color[] farmerPixelsToRecolour = farmerBaseCustomised.GetPixels(0, 0, 288, farmerBaseTexture.height);

        // Populate Skin Color Swap List
        PopulateSkinColorSwapList(inputSkinType);

        // Change skin colours
        ChangePixelColors(farmerPixelsToRecolour, colorSwapList);

        // Set recoloured pixels
        farmerBaseCustomised.SetPixels(0, 0, 288, farmerBaseTexture.height, farmerPixelsToRecolour);

        // Apply texture changes
        farmerBaseCustomised.Apply();
    }

    private void ProcessHat()
    {
        // Create Selected Hat Texture
        AddHatToTexture(inputHatStyleNo);
    }

    private void ProcessAdornments()
    {
        // Initialise body adornments offset array;
        bodyAdornmentsOffsetArray = new Vector2Int[bodyColumns, bodyRows];

        // Populate body adornments offset array
        PopulateBodyAdornmentsOffsetArray();

        // Create Selected Adornments Texture
        AddAdornmentsToTexture(inputAdornmentsStyleNo);

        //Create new adornments base texture
        farmerBaseAdornmentsUpdated = new Texture2D(farmerBaseTexture.width, farmerBaseTexture.height);
        farmerBaseAdornmentsUpdated.filterMode = FilterMode.Point;

        // Set adornments base texture to transparent
        SetTextureToTransparent(farmerBaseAdornmentsUpdated);
        ApplyAdornmentsTextureToBase();
    }

    private void MergeCustomisations()
    {
        // Farmer Shirt Pixels
        Color[] farmerShirtPixels =
            farmerBaseShirtsUpdated.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);
        // Farmer Trouser Pixels
        Color[] farmerTrouserPixelsSelection = farmerBaseCustomised.GetPixels(288, 0, 96, farmerBaseTexture.height);

        Color[] farmerAdornmentsPixels =
            farmerBaseAdornmentsUpdated.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);
        
        // Farmer Body Pixels
        Color[] farmerBodyPixels =
            farmerBaseCustomised.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);

        MergeColourArray(farmerBodyPixels, farmerTrouserPixelsSelection);
        MergeColourArray(farmerBodyPixels, farmerShirtPixels);
        MergeColourArray(farmerBodyPixels, farmerAdornmentsPixels);

        // Paste merged  pixels
        farmerBaseCustomised.SetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height,
            farmerBodyPixels);

        // Apply texture changes
        farmerBaseCustomised.Apply();
    }

    #endregion

    
    

    #region ProcessShirt调用的方法
    
    private void AddShirtToTexture(int shirtStyleNo)
    {
        // Create shirt texture
        selectedShirt = new Texture2D(shirtTextureWidth, shirtTextureHeight);
        selectedShirt.filterMode = FilterMode.Point;

        // Calculate coordinates for shirt pixels
        int y = (shirtStyleNo / shirtStylesInSpriteWidth) * shirtTextureHeight;
        int x = (shirtStyleNo % shirtStylesInSpriteWidth) * shirtTextureWidth;

        // Get shirts pixels
        Color[] shirtPixels = shirtsBaseTexture.GetPixels(x, y, shirtTextureWidth, shirtTextureHeight);

        // Apply selected shirt pixels to texture
        selectedShirt.SetPixels(shirtPixels);
        selectedShirt.Apply();
    }

    private void ApplyShirtTextureToBase()
    {
        // Create new shirt base texture
        farmerBaseShirtsUpdated = new Texture2D(farmerBaseTexture.width, farmerBaseTexture.height);
        farmerBaseShirtsUpdated.filterMode = FilterMode.Point;

        // Set shirt base texture to transparent
        SetTextureToTransparent(farmerBaseShirtsUpdated);

        Color[] frontShirtPixels;
        Color[] backShirtPixels;
        Color[] rightShirtPixels;

        frontShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 3, shirtSpriteWidth, shirtSpriteHeight);
        backShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 0, shirtSpriteWidth, shirtSpriteHeight);
        rightShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 2, shirtSpriteWidth, shirtSpriteHeight);

        // Loop through base texture and apply shirt pixels
        for (int x = 0; x < bodyColumns; x++)
        {
            for (int y = 0; y < bodyRows; y++)
            {
                int pixelX = x * farmerSpriteWidth;
                int pixelY = y * farmerSpriteHeight;

                if (bodyShirtOffsetArray[x, y] != null)
                {
                    if (bodyShirtOffsetArray[x, y].x == 99 && bodyShirtOffsetArray[x, y].y == 99) // do not populate with shirt
                        continue;

                    pixelX += bodyShirtOffsetArray[x, y].x;
                    pixelY += bodyShirtOffsetArray[x, y].y;
                }

                // Switch on facing direction
                switch (bodyFacingArray[x, y])
                {
                    case Facing.none:
                        break;

                    case Facing.front:
                        // Populate front shirt pixels
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, frontShirtPixels);
                        break;

                    case Facing.back:
                        // Populate back shirt pixels
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, backShirtPixels);
                        break;

                    case Facing.right:
                        // Populate right shirt pixels
                        farmerBaseShirtsUpdated.SetPixels(pixelX, pixelY, shirtSpriteWidth, shirtSpriteHeight, rightShirtPixels);
                        break;

                    default:
                        break;
                }
            }
        }

        // Apply shirt texture pixels
        farmerBaseShirtsUpdated.Apply();
    }
    
    private void SetTextureToTransparent(Texture2D texture2D)
    {
        // Fill texture with transparency
        Color[] fill = new Color[texture2D.height * texture2D.width];
        for (int i = 0; i < fill.Length; i++)
        {
            fill[i] = Color.clear;
        }
        texture2D.SetPixels(fill);
    }
    
    private void PopulateBodyFacingArray()
    {
        bodyFacingArray[0, 0] = Facing.none;
        bodyFacingArray[1, 0] = Facing.none;
        bodyFacingArray[2, 0] = Facing.none;
        bodyFacingArray[3, 0] = Facing.none;
        bodyFacingArray[4, 0] = Facing.none;
        bodyFacingArray[5, 0] = Facing.none;

        bodyFacingArray[0, 1] = Facing.none;
        bodyFacingArray[1, 1] = Facing.none;
        bodyFacingArray[2, 1] = Facing.none;
        bodyFacingArray[3, 1] = Facing.none;
        bodyFacingArray[4, 1] = Facing.none;
        bodyFacingArray[5, 1] = Facing.none;

        bodyFacingArray[0, 2] = Facing.none;
        bodyFacingArray[1, 2] = Facing.none;
        bodyFacingArray[2, 2] = Facing.none;
        bodyFacingArray[3, 2] = Facing.none;
        bodyFacingArray[4, 2] = Facing.none;
        bodyFacingArray[5, 2] = Facing.none;

        bodyFacingArray[0, 3] = Facing.none;
        bodyFacingArray[1, 3] = Facing.none;
        bodyFacingArray[2, 3] = Facing.none;
        bodyFacingArray[3, 3] = Facing.none;
        bodyFacingArray[4, 3] = Facing.none;
        bodyFacingArray[5, 3] = Facing.none;

        bodyFacingArray[0, 4] = Facing.none;
        bodyFacingArray[1, 4] = Facing.none;
        bodyFacingArray[2, 4] = Facing.none;
        bodyFacingArray[3, 4] = Facing.none;
        bodyFacingArray[4, 4] = Facing.none;
        bodyFacingArray[5, 4] = Facing.none;

        bodyFacingArray[0, 5] = Facing.none;
        bodyFacingArray[1, 5] = Facing.none;
        bodyFacingArray[2, 5] = Facing.none;
        bodyFacingArray[3, 5] = Facing.none;
        bodyFacingArray[4, 5] = Facing.none;
        bodyFacingArray[5, 5] = Facing.none;

        bodyFacingArray[0, 6] = Facing.none;
        bodyFacingArray[1, 6] = Facing.none;
        bodyFacingArray[2, 6] = Facing.none;
        bodyFacingArray[3, 6] = Facing.none;
        bodyFacingArray[4, 6] = Facing.none;
        bodyFacingArray[5, 6] = Facing.none;

        bodyFacingArray[0, 7] = Facing.none;
        bodyFacingArray[1, 7] = Facing.none;
        bodyFacingArray[2, 7] = Facing.none;
        bodyFacingArray[3, 7] = Facing.none;
        bodyFacingArray[4, 7] = Facing.none;
        bodyFacingArray[5, 7] = Facing.none;

        bodyFacingArray[0, 8] = Facing.none;
        bodyFacingArray[1, 8] = Facing.none;
        bodyFacingArray[2, 8] = Facing.none;
        bodyFacingArray[3, 8] = Facing.none;
        bodyFacingArray[4, 8] = Facing.none;
        bodyFacingArray[5, 8] = Facing.none;

        bodyFacingArray[0, 9] = Facing.none;
        bodyFacingArray[1, 9] = Facing.none;
        bodyFacingArray[2, 9] = Facing.none;
        bodyFacingArray[3, 9] = Facing.none;
        bodyFacingArray[4, 9] = Facing.none;
        bodyFacingArray[5, 9] = Facing.none;

        bodyFacingArray[0, 10] = Facing.back;
        bodyFacingArray[1, 10] = Facing.back;
        bodyFacingArray[2, 10] = Facing.right;
        bodyFacingArray[3, 10] = Facing.right;
        bodyFacingArray[4, 10] = Facing.right;
        bodyFacingArray[5, 10] = Facing.right;

        bodyFacingArray[0, 11] = Facing.front;
        bodyFacingArray[1, 11] = Facing.front;
        bodyFacingArray[2, 11] = Facing.front;
        bodyFacingArray[3, 11] = Facing.front;
        bodyFacingArray[4, 11] = Facing.back;
        bodyFacingArray[5, 11] = Facing.back;

        bodyFacingArray[0, 12] = Facing.back;
        bodyFacingArray[1, 12] = Facing.back;
        bodyFacingArray[2, 12] = Facing.right;
        bodyFacingArray[3, 12] = Facing.right;
        bodyFacingArray[4, 12] = Facing.right;
        bodyFacingArray[5, 12] = Facing.right;

        bodyFacingArray[0, 13] = Facing.front;
        bodyFacingArray[1, 13] = Facing.front;
        bodyFacingArray[2, 13] = Facing.front;
        bodyFacingArray[3, 13] = Facing.front;
        bodyFacingArray[4, 13] = Facing.back;
        bodyFacingArray[5, 13] = Facing.back;

        bodyFacingArray[0, 14] = Facing.back;
        bodyFacingArray[1, 14] = Facing.back;
        bodyFacingArray[2, 14] = Facing.right;
        bodyFacingArray[3, 14] = Facing.right;
        bodyFacingArray[4, 14] = Facing.right;
        bodyFacingArray[5, 14] = Facing.right;

        bodyFacingArray[0, 15] = Facing.front;
        bodyFacingArray[1, 15] = Facing.front;
        bodyFacingArray[2, 15] = Facing.front;
        bodyFacingArray[3, 15] = Facing.front;
        bodyFacingArray[4, 15] = Facing.back;
        bodyFacingArray[5, 15] = Facing.back;

        bodyFacingArray[0, 16] = Facing.back;
        bodyFacingArray[1, 16] = Facing.back;
        bodyFacingArray[2, 16] = Facing.right;
        bodyFacingArray[3, 16] = Facing.right;
        bodyFacingArray[4, 16] = Facing.right;
        bodyFacingArray[5, 16] = Facing.right;

        bodyFacingArray[0, 17] = Facing.front;
        bodyFacingArray[1, 17] = Facing.front;
        bodyFacingArray[2, 17] = Facing.front;
        bodyFacingArray[3, 17] = Facing.front;
        bodyFacingArray[4, 17] = Facing.back;
        bodyFacingArray[5, 17] = Facing.back;

        bodyFacingArray[0, 18] = Facing.back;
        bodyFacingArray[1, 18] = Facing.back;
        bodyFacingArray[2, 18] = Facing.back;
        bodyFacingArray[3, 18] = Facing.right;
        bodyFacingArray[4, 18] = Facing.right;
        bodyFacingArray[5, 18] = Facing.right;

        bodyFacingArray[0, 19] = Facing.right;
        bodyFacingArray[1, 19] = Facing.right;
        bodyFacingArray[2, 19] = Facing.right;
        bodyFacingArray[3, 19] = Facing.front;
        bodyFacingArray[4, 19] = Facing.front;
        bodyFacingArray[5, 19] = Facing.front;

        bodyFacingArray[0, 20] = Facing.front;
        bodyFacingArray[1, 20] = Facing.front;
        bodyFacingArray[2, 20] = Facing.front;
        bodyFacingArray[3, 20] = Facing.back;
        bodyFacingArray[4, 20] = Facing.back;
        bodyFacingArray[5, 20] = Facing.back;
    }

    private void PopulateBodyShirtOffsetArray()
    {
        bodyShirtOffsetArray[0, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 0] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 1] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 2] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 3] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 4] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 5] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 6] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 7] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 8] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 9] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[1, 10] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[2, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[3, 10] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[4, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[5, 10] = new Vector2Int(4, 10);

        bodyShirtOffsetArray[0, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[1, 11] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[2, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[3, 11] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[5, 11] = new Vector2Int(4, 12);

        bodyShirtOffsetArray[0, 12] = new Vector2Int(3, 9);
        bodyShirtOffsetArray[1, 12] = new Vector2Int(3, 9);
        bodyShirtOffsetArray[2, 12] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[3, 12] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[4, 12] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 12] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 13] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 13] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 13] = new Vector2Int(5, 9);
        bodyShirtOffsetArray[3, 13] = new Vector2Int(5, 9);
        bodyShirtOffsetArray[4, 13] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[5, 13] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 14] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[1, 14] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[2, 14] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[3, 14] = new Vector2Int(4, 5);
        bodyShirtOffsetArray[4, 14] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 14] = new Vector2Int(4, 12);

        bodyShirtOffsetArray[0, 15] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[1, 15] = new Vector2Int(4, 5);
        bodyShirtOffsetArray[2, 15] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 15] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[4, 15] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[5, 15] = new Vector2Int(4, 5);

        bodyShirtOffsetArray[0, 16] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[1, 16] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[2, 16] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[3, 16] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[4, 16] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 16] = new Vector2Int(4, 10);

        bodyShirtOffsetArray[0, 17] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[1, 17] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[2, 17] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 17] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 17] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[5, 17] = new Vector2Int(4, 8);

        bodyShirtOffsetArray[0, 18] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 18] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 18] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 19] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 19] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 19] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 20] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 20] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 20] = new Vector2Int(4, 9);
    }
    
    #endregion
    
    
    
    
    #region ProcessArms调用的方法
    
    private void PopulateArmColorSwapList()
    {
        // Clear color swap list
        colorSwapList.Clear();

        // Arm replacement colors
        colorSwapList.Add(new colorSwap(armTargetColor1, selectedShirt.GetPixel(0, 7)));
        colorSwapList.Add(new colorSwap(armTargetColor2, selectedShirt.GetPixel(0, 6)));
        colorSwapList.Add(new colorSwap(armTargetColor3, selectedShirt.GetPixel(0, 5)));
    }

    private void ChangePixelColors(Color[] baseArray, List<colorSwap> colorSwapList)
    {
        for (int i = 0; i < baseArray.Length; i++)
        {
            // Loop through color swap list
            if (colorSwapList.Count > 0)
            {
                for (int j = 0; j < colorSwapList.Count; j++)
                {
                    if (isSameColor(baseArray[i], colorSwapList[j].fromColor))
                    {
                        baseArray[i] = colorSwapList[j].toColor;
                    }
                }
            }
        }
    }
    
    private bool isSameColor(Color color1, Color color2)
    {
        if ((color1.r == color2.r) && (color1.g == color2.g) && (color1.b == color2.b) && (color1.a == color2.a))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion


    
    
    #region ProcessTrousers调用到的方法

    private void TintPixelColors(Color[] basePixelArray, Color tintColor)
    {
        // Loop through pixels to tint
        for (int i = 0; i < basePixelArray.Length; i++)
        {
            basePixelArray[i].r = basePixelArray[i].r * tintColor.r;
            basePixelArray[i].g = basePixelArray[i].g * tintColor.g;
            basePixelArray[i].b = basePixelArray[i].b * tintColor.b;
        }
    }

    #endregion

    
    
    
    #region ProcessHair调用到的方法

    private void AddHairToTexture(int hairStyleNo)
    {
        // Calculate coordinates for hair pixels
        int y = (hairStyleNo / hairStylesInSpriteWidth) * hairTextureHeight;
        int x = (hairStyleNo % hairStylesInSpriteWidth) * hairTextureWidth;

        // Get hair pixels
        Color[] hairPixels = hairBaseTexture.GetPixels(x, y, hairTextureWidth, hairTextureHeight);

        // Apply selected hair pixels to texture
        hairCustomised.SetPixels(hairPixels);
        hairCustomised.Apply();
    }
    
    #endregion


    
    
    #region ProcessSkin调用到的方法

    private void PopulateSkinColorSwapList(int skinType)
    {
        // Clear color swap list
        colorSwapList.Clear();

        // Skin replacement colors
        // Switch on skin type
        switch (skinType)
        {
            case 0:
                colorSwapList.Add(new colorSwap(skinTargetColor1, skinTargetColor1));
                colorSwapList.Add(new colorSwap(skinTargetColor2, skinTargetColor2));
                colorSwapList.Add(new colorSwap(skinTargetColor3, skinTargetColor3));
                colorSwapList.Add(new colorSwap(skinTargetColor4, skinTargetColor4));
                break;

            case 1:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(187, 157, 128, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(231, 187, 144, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(221, 186, 154, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(213, 189, 167, 255)));
                break;

            case 2:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(105, 69, 2, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(128, 87, 12, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(145, 103, 26, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(161, 114, 25, 255)));
                break;

            case 3:
                colorSwapList.Add(new colorSwap(skinTargetColor1, new Color32(151, 132, 0, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor2, new Color32(187, 166, 15, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor3, new Color32(209, 188, 39, 255)));
                colorSwapList.Add(new colorSwap(skinTargetColor4, new Color32(211, 199, 112, 255)));
                break;

            default:
                colorSwapList.Add(new colorSwap(skinTargetColor1, skinTargetColor1));
                colorSwapList.Add(new colorSwap(skinTargetColor2, skinTargetColor2));
                colorSwapList.Add(new colorSwap(skinTargetColor3, skinTargetColor3));
                colorSwapList.Add(new colorSwap(skinTargetColor4, skinTargetColor4));
                break;
        }
    }

    #endregion




    #region ProcessHat调用到的方法

    private void AddHatToTexture(int hatStyleNo)
    {
        // Calculate coordinates for hat pixels
        int y = (hatStyleNo / hatStylesInSpriteWidth) * hatTextureHeight;
        int x = (hatStyleNo % hatStylesInSpriteWidth) * hatTextureWidth;

        // Get hat pixels
        Color[] hatPixels = hatsBaseTexture.GetPixels(x, y, hatTextureWidth, hatTextureHeight);

        // Apply selected hat pixels to texture
        hatsCustomised.SetPixels(hatPixels);
        hatsCustomised.Apply();
    }

    #endregion




    #region ProcessAdornments调用到的方法

    private void PopulateBodyAdornmentsOffsetArray()
    {
        bodyAdornmentsOffsetArray[0, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 0] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 0] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 1] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 1] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 2] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 2] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 3] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 3] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 4] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 4] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 5] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 5] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 6] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 6] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 7] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 7] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 8] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 8] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 9] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 9] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 10] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 10] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 10] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[3, 10] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[4, 10] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[5, 10] = new Vector2Int(0, 0 + 16);

        bodyAdornmentsOffsetArray[0, 11] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[1, 11] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[2, 11] = new Vector2Int(0, 1 + 16);
        bodyAdornmentsOffsetArray[3, 11] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 11] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 11] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 12] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 12] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 12] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[3, 12] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[4, 12] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 12] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 13] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 13] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 13] = new Vector2Int(1, -1 + 16);
        bodyAdornmentsOffsetArray[3, 13] = new Vector2Int(1, -1 + 16);
        bodyAdornmentsOffsetArray[4, 13] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 13] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 14] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 14] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 14] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[3, 14] = new Vector2Int(0, -5 + 16);
        bodyAdornmentsOffsetArray[4, 14] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 14] = new Vector2Int(0, 1 + 16);

        bodyAdornmentsOffsetArray[0, 15] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[1, 15] = new Vector2Int(0, -5 + 16);
        bodyAdornmentsOffsetArray[2, 15] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 15] = new Vector2Int(0, 2 + 16);
        bodyAdornmentsOffsetArray[4, 15] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 15] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 16] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 16] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 16] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[3, 16] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[4, 16] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 16] = new Vector2Int(0, 0 + 16);

        bodyAdornmentsOffsetArray[0, 17] = new Vector2Int(0, -3 + 16);
        bodyAdornmentsOffsetArray[1, 17] = new Vector2Int(0, -2 + 16);
        bodyAdornmentsOffsetArray[2, 17] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 17] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 17] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 17] = new Vector2Int(99, 99);

        bodyAdornmentsOffsetArray[0, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[1, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[2, 18] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[3, 18] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 18] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 18] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 19] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 19] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[4, 19] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[5, 19] = new Vector2Int(0, -1 + 16);

        bodyAdornmentsOffsetArray[0, 20] = new Vector2Int(0, 0 + 16);
        bodyAdornmentsOffsetArray[1, 20] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[2, 20] = new Vector2Int(0, -1 + 16);
        bodyAdornmentsOffsetArray[3, 20] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[4, 20] = new Vector2Int(99, 99);
        bodyAdornmentsOffsetArray[5, 20] = new Vector2Int(99, 99);
    }
    
    private void AddAdornmentsToTexture(int adornmentsStyleNo)
    {
        // Create adornment texture
        selectedAdornment = new Texture2D(adornmentsTextureWidth, adornmentsTextureHeight);
        selectedAdornment.filterMode = FilterMode.Point;

        // Calculate coordinates for adornments pixels
        int y = (adornmentsStyleNo / adornmentsStylesInSpriteWidth) * adornmentsTextureHeight;
        int x = (adornmentsStyleNo % adornmentsStylesInSpriteWidth) * adornmentsTextureWidth;

        // Get adornments pixels
        Color[] adornmentsPixels = adornmentsBaseTexture.GetPixels(x, y, adornmentsTextureWidth, adornmentsTextureHeight);

        // Apply selected adornments pixels to texture
        selectedAdornment.SetPixels(adornmentsPixels);
        selectedAdornment.Apply();
    }
    
    private void ApplyAdornmentsTextureToBase()
    {
        Color[] frontAdornmentsPixels;
        Color[] rightAdornmentsPixels;

        frontAdornmentsPixels = selectedAdornment.GetPixels(0, adornmentsSpriteHeight * 1, adornmentsSpriteWidth,
            adornmentsSpriteHeight);

        rightAdornmentsPixels = selectedAdornment.GetPixels(0, adornmentsSpriteHeight * 0, adornmentsSpriteWidth,
            adornmentsSpriteHeight);

        // Loop through base texture and apply adornments pixels

        for (int x = 0; x < bodyColumns; x++)
        {
            for (int y = 0; y < bodyRows; y++)
            {
                int pixelX = x * farmerSpriteWidth;
                int pixelY = y * farmerSpriteHeight;

                if (bodyAdornmentsOffsetArray[x, y] != null)
                {
                    pixelX += bodyAdornmentsOffsetArray[x, y].x;
                    pixelY += bodyAdornmentsOffsetArray[x, y].y;
                }

                // Switch on facing direction
                switch (bodyFacingArray[x, y])
                {
                    case Facing.none:
                        break;

                    case Facing.front:
                        // Populate front adornments pixels
                        farmerBaseAdornmentsUpdated.SetPixels(pixelX, pixelY, adornmentsSpriteWidth,
                            adornmentsSpriteHeight, frontAdornmentsPixels);
                        break;

                    case Facing.right:
                        // Populate right adornments pixels
                        farmerBaseAdornmentsUpdated.SetPixels(pixelX, pixelY, adornmentsSpriteWidth,
                            adornmentsSpriteHeight, rightAdornmentsPixels);
                        break;

                    default:
                        break;
                }
            }
        }

        // Apply adornments texture pixels
        farmerBaseAdornmentsUpdated.Apply();
    }


    #endregion
    
    
    
    
    #region MergeCustomisations调用到的方法

    private void MergeColourArray(Color[] baseArray, Color[] mergeArray)
    {
        for (int i = 0; i < baseArray.Length; i++)
        {
            if (mergeArray[i].a > 0)
            {
                //Merge array has color
                if (mergeArray[i].a >= 1)
                {
                    // Fully replace
                    baseArray[i] = mergeArray[i];
                }
                else
                {
                    //Interpolate colors
                    float alpha = mergeArray[i].a;

                    baseArray[i].r += (mergeArray[i].r - baseArray[i].r) * alpha;
                    baseArray[i].g += (mergeArray[i].g - baseArray[i].g) * alpha;
                    baseArray[i].b += (mergeArray[i].b - baseArray[i].b) * alpha;
                    baseArray[i].a += mergeArray[i].a;
                }
            }
        }
    }
    #endregion
}
