
public static class GridPosExtensions
{
    /// <summary>
    /// 将GridPos转换为int数组 [col, row]
    /// </summary>
    public static int[] ToArray(this GridPos gridPos)
    {
        return new int[] { gridPos.col, gridPos.row };
    }
}