package com.agenthub.android

import kotlinx.coroutines.channels.Channel

/** The WebSocket reader waits for capacity; the coroutine receiver never blocks Main.
 * Closing releases a reader waiting for either the frame or byte budget.
 */
internal class EasyWorkInboundFrames(
    private val maxFrameBytes: Int=32*1024*1024,
    private val maxBufferedBytes: Long=64L*1024*1024,
    private val capacity: Int=64,
) {
    init { require(maxFrameBytes>0 && maxBufferedBytes>=maxFrameBytes && capacity>0) }
    private val changed=Object()
    private val input=Channel<ByteArray>(capacity,onUndeliveredElement=::released)
    private var buffered=0L
    private var queued=0
    private var stopped=false

    /** Called only on OkHttp's dedicated reader, where waiting propagates TCP backpressure. */
    fun accept(bytes: ByteArray) = synchronized(changed) {
        require(bytes.size<=maxFrameBytes) { "电脑消息超过接收上限" }
        while(!stopped && (queued>=capacity || buffered+bytes.size>maxBufferedBytes))changed.wait()
        check(!stopped) { "电脑通道已关闭" }
        buffered+=bytes.size;queued++
        check(input.trySend(bytes).isSuccess) { "电脑接收队列已关闭" }
    }

    private fun released(bytes: ByteArray) = synchronized(changed) {
        buffered-=bytes.size;queued--;changed.notifyAll()
    }

    suspend fun receive(): ByteArray = input.receive().also(::released)

    fun close(cause: Throwable?=null,discard: Boolean=false) = synchronized(changed) {
        stopped=true
        if(discard)input.cancel() else input.close(cause)
        changed.notifyAll()
    }
}
