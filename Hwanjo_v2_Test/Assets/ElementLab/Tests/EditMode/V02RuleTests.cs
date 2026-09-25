using NUnit.Framework;
namespace Hwanjo.ElementLab.Tests
{
    public class V02RuleTests
    {
        [TestCase(1,true,false,SlashDirection.Up)][TestCase(-1,true,false,SlashDirection.Up)]
        [TestCase(-1,false,true,SlashDirection.Down)][TestCase(-1,true,true,SlashDirection.Left)]
        [TestCase(1,false,false,SlashDirection.Right)]
        public void CardinalInputPriority(int facing,bool up,bool down,SlashDirection expected) => Assert.AreEqual(expected,AttackDirections.Resolve(facing,up,down));
        [Test] public void HelpTableAndTracePolicyShareEveryCell()
        {
            for(int row=0;row<5;row++)for(int col=0;col<4;col++)
            {
                Element source=(Element)System.Math.Min(row,3), input=(Element)col;
                Assert.IsNotEmpty(TraceSupport.Description(source,row==4,input));
                if(TraceSupport.Get(source,row==4,input)==TraceResponse.Unsupported)
                    Assert.That(TraceSupport.Description(source,row==4,input),Does.Contain(row==4?"이동 불가":"미지원"));
            }
            Assert.AreEqual(TraceResponse.End,TraceSupport.Get(Element.Ice,false,Element.Fire));
            Assert.AreEqual(TraceResponse.Thaw,TraceSupport.Get(Element.Ice,true,Element.Fire));
        }
    }
}
