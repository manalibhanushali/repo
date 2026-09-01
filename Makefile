DOTNET ?= $(shell command -v dotnet 2>/dev/null || echo "$(HOME)/.dotnet/dotnet")
SOLUTION := LoanIQ.Integration.slnx
CONFIGURATION ?= Release

.PHONY: all build test run openapi audit clean

all: build test

build:
	$(DOTNET) build $(SOLUTION) --configuration $(CONFIGURATION)

test:
	$(DOTNET) test $(SOLUTION) --configuration $(CONFIGURATION) --no-build

run:
	$(DOTNET) run --project src/LoanIQ.Integration.Api --configuration Development

openapi:
	bash .polaira/emit-openapi.sh

audit:
	$(DOTNET) list package --vulnerable --include-transitive

clean:
	$(DOTNET) clean $(SOLUTION)
	find . -type d \( -name bin -o -name obj \) -not -path './.git/*' | xargs rm -rf
