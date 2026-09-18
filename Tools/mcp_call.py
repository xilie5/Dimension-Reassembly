import asyncio
import json
import sys

from mcp import ClientSession
from mcp.client.streamable_http import streamablehttp_client


async def main() -> int:
    if len(sys.argv) < 2:
        print("usage: mcp_call.py <tool-name> [json-arguments]", file=sys.stderr)
        return 2

    tool_name = sys.argv[1]
    arguments = json.loads(sys.argv[2]) if len(sys.argv) > 2 else {}
    endpoint = "http://127.0.0.1:1534/mcp"

    async with streamablehttp_client(endpoint) as (read_stream, write_stream, _):
        async with ClientSession(read_stream, write_stream) as session:
            await session.initialize()
            result = await session.call_tool(tool_name, arguments)
            print(json.dumps(result.model_dump(mode="json"), ensure_ascii=False, indent=2))

    return 0


if __name__ == "__main__":
    raise SystemExit(asyncio.run(main()))
