

public struct CharacterAttribute
{
    public CharacterPartAnimator characterPart; //指明动画适用于何种部位，手或者身体
    public PartVariantColour partVariantColour; //指明动画的颜色
    public PartVariantType partVariantType; //指明动画的类型，拿着或者锄地

    public CharacterAttribute(CharacterPartAnimator characterPart, PartVariantColour partVariantColour,
        PartVariantType partVariantType)
    {
        this.characterPart = characterPart;
        this.partVariantColour = partVariantColour;
        this.partVariantType = partVariantType;
    }
}
