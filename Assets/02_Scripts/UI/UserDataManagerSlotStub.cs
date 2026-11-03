// 임시 스텁: UserDataManager에 실제 메서드가 생기면 이 파일을 삭제한다.
// (같은 이름의 인스턴스 메서드가 있으면 컴파일러가 그쪽을 우선하므로 충돌하지 않는다.)
using System;

public static class UserDataManagerSlotStub
{
    public static bool HasSave(this UserDataManager manager, int slot) { return false; }
    public static DateTime? GetSaveTime(this UserDataManager manager, int slot) { return null; }
    public static void SelectSlot(this UserDataManager manager, int slot) { }
    public static bool DeleteSlot(this UserDataManager manager, int slot) { return false; }
}