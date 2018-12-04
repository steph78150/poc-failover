using System;
using Xunit;
using NFluent;

namespace poc_failover.Tests
{

    public class ElectionStateTests
    {
        [Fact]
        public void Should_be_able_to_vote_once()
        {
            // Given
            var state = ElectionState.Empty();
            // When
            bool hadVoted = state.TryToVoteFor(CandidateObjectMother.BillClinton())
            ;
            // Then
            Check.That(hadVoted).IsTrue();
        }

        [Fact]
        public void Should_not_be_able_to_vote_twice_in_the_same_term()
        {
            // Given
            var state = ElectionState.Empty();
            state.TryToVoteFor(CandidateObjectMother.BarackObama());

            // When
            bool hadVoted = state.TryToVoteFor(CandidateObjectMother.MittRomney());
            // Then
            Check.That(hadVoted).IsFalse();
        }
        
        [Fact]
        public void Should_increase_term_when_newer_term() 
        {
            // Given
            var state = ElectionState.Empty();
            state.TryToVoteFor(CandidateObjectMother.BillClinton());
            // When
            var hadVoted = state.TryToVoteFor(CandidateObjectMother.BarackObama());
            // Then
            Check.That(hadVoted).IsTrue();
            Check.That(state.Term).IsEqualTo(2012);
        }

        [Fact]
        public void Should_not_increase_term_when_older_term() 
        {
            // Given
            var state = ElectionState.Empty();
            state.TryToVoteFor(CandidateObjectMother.BillClinton());
            // When
            var hadVoted = state.TryToVoteFor(CandidateObjectMother.JfKennedy());
            // Then
            Check.That(hadVoted).IsFalse();
            Check.That(state.Term).IsEqualTo(1992);
        }      

        [Fact]
        public void Should_increment_term_when_organizing_election() 
        {
            var state = ElectionState.Empty();
            var oldTerm = state.Term;

            state.NewElection("me");

            Check.That(state.Term).IsEqualTo(oldTerm + 1);
            Check.That(state.VotedFor).IsEqualTo("me");
        }  
    }
}
