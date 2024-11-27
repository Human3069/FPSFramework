namespace VTSFramework.TSModule
{
    public interface IRow<T> where T : IRow<T>
    {
        // 테이블의 내용을 정제하여 리턴해주는 함수
        // 예) 띄워쓰기 및 \n, 대소문자를 정규화해줌.
        T Validated();
    }
}