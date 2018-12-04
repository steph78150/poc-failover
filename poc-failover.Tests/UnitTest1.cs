using System;
using Xunit;
using NFluent;

namespace poc_failover.Tests
{
    public class ElectionStateTests
    {
        [Fact]
        public void Should_not_be_able_to_vote_twice_in_the_same_term()
        {
            var state = ElectionState.Empty();
            state.SeenTerm(42);

            Check.That(state.TryToVoteFor("george_bush")).IsTrue();
            Check.That(state.TryToVoteFor("barack_obama")).IsFalse();
        }

        
    }
}
