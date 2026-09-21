using System.Runtime.CompilerServices;
using TubaWinUi3.Services.Ai;

namespace TubaWinUi3.Tests
{
    /// <summary>
    /// AI 助手面板（FieldCure ChatPanel）销毁竞态异常的识别（Issue #194）。
    /// 认错了会吞掉工具箱自己的真问题，认漏了用户又会反复看到"未处理异常"错误窗口。
    /// </summary>
    public class ChatPanelCrashFilterTests
    {
        [Fact]
        public void IsTeardownRace_AcceptsChatPanelClosedWebViewException()
            => Assert.True(ChatPanelCrashFilter.IsTeardownRace(
                Capture(() => FieldCure.AssistStudio.Fakes.ChatPanelRenderer.ThrowClosedWebView())));

        [Fact]
        public void IsTeardownRace_RejectsSameMessageThrownByOwnCode()
            => Assert.False(ChatPanelCrashFilter.IsTeardownRace(Capture(ThrowClosedWebViewLocally)));

        [Fact]
        public void IsTeardownRace_RejectsOtherChatPanelFailures()
            => Assert.False(ChatPanelCrashFilter.IsTeardownRace(
                Capture(() => FieldCure.AssistStudio.Fakes.ChatPanelRenderer.ThrowSomethingElse())));

        [Fact]
        public void IsTeardownRace_RejectsMissingException()
            => Assert.False(ChatPanelCrashFilter.IsTeardownRace(null));

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowClosedWebViewLocally()
            => throw new InvalidOperationException(
                "在意外的时间调用了方法。执行脚本失败：CoreWebView2 is not present.");

        private static Exception Capture(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                return ex;
            }
            throw new InvalidOperationException("预期被测委托抛出异常，但没有");
        }
    }
}

namespace FieldCure.AssistStudio.Fakes
{
    /// <summary>冒充组件库的抛出点，让异常栈里带上 FieldCure.AssistStudio 命名空间。</summary>
    internal static class ChatPanelRenderer
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowClosedWebView()
            => throw new InvalidOperationException(
                "在意外的时间调用了方法。\r\nExecuteScriptAsync(): Failed because a valid CoreWebView2 is not present. " +
                "Make sure one was created, for example by calling EnsureCoreWebView2Async() API.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowSomethingElse()
            => throw new InvalidOperationException("在意外的时间调用了方法。");
    }
}
