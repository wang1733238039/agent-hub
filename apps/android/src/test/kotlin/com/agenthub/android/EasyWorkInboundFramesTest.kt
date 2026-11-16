package com.agenthub.android

import kotlinx.coroutines.*
import org.junit.Assert.*
import org.junit.Test
import java.util.concurrent.CountDownLatch
import java.util.concurrent.TimeUnit

/** Drives the production queue with a reader faster than its coroutine consumer. */
class EasyWorkInboundFramesTest {
    @Test fun burstWaitsForCapacityAndKeepsEveryFrameInOrder()=runBlocking {
        val queue=EasyWorkInboundFrames(8,16,2)
        val started=CountDownLatch(1)
        val finished=CountDownLatch(1)
        val producer=async(Dispatchers.IO) {
            for(i in 0 until 200) {
                if(i==2)started.countDown()
                queue.accept(byteArrayOf(i.toByte()))
            }
            finished.countDown()
        }
        try {
            assertTrue(started.await(2,TimeUnit.SECONDS))
            assertFalse(finished.await(50,TimeUnit.MILLISECONDS))
            for(i in 0 until 200)assertEquals(i.toByte(),withTimeout(2000) {queue.receive()}.single())
            withTimeout(2000) {producer.await()}
        } finally {queue.close(discard=true);producer.cancel()}
    }

    @Test fun byteBudgetAppliesBeforeTheFrameCountLimit()=runBlocking {
        val queue=EasyWorkInboundFrames(8,8,64)
        queue.accept(ByteArray(8))
        val started=CountDownLatch(1)
        val producer=async(Dispatchers.IO) {started.countDown();queue.accept(byteArrayOf(7))}
        try {
            assertTrue(started.await(2,TimeUnit.SECONDS))
            delay(50);assertFalse(producer.isCompleted)
            assertEquals(8,queue.receive().size)
            withTimeout(2000) {producer.await()}
            assertEquals(7,queue.receive().single().toInt())
        } finally {queue.close(discard=true);producer.cancel()}
    }

    @Test fun closingReleasesAReaderWaitingForSpace()=runBlocking {
        val queue=EasyWorkInboundFrames(8,8,1)
        queue.accept(byteArrayOf(1))
        val started=CountDownLatch(1)
        val producer=async(Dispatchers.IO) {started.countDown();runCatching {queue.accept(byteArrayOf(2))}}
        assertTrue(started.await(2,TimeUnit.SECONDS))
        queue.close(discard=true)
        assertTrue(withTimeout(2000) {producer.await()}.isFailure)
    }

    @Test fun oversizeRejectionDoesNotDiscardAcceptedFrames()=runBlocking {
        val queue=EasyWorkInboundFrames(2,2,1)
        queue.accept(byteArrayOf(1))
        assertTrue(runCatching {queue.accept(ByteArray(3))}.isFailure)
        assertEquals(1,queue.receive().single().toInt())
        queue.close()
        assertTrue(runCatching {queue.receive()}.isFailure)
    }
}
